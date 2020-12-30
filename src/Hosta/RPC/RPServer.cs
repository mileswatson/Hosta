using Hosta.Crypto;
using Hosta.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.RPC
{
	/// <summary>
	/// A server that handles API requests.
	/// </summary>
	public class RPServer : IDisposable
	{
		/// <summary>
		/// Handles procedure calls.
		/// </summary>
		public readonly ICallable callHandler;

		/// <summary>
		/// Underlying Socket Server to listen for incoming
		/// connection requests.
		/// </summary>
		private readonly SocketServer listener;

		/// <summary>
		/// Used for authenticating the client.
		/// </summary>
		private readonly Authenticator authenticator;

		/// <summary>
		/// Set of current connections.
		/// </summary>
		private readonly HashSet<IDisposable> connections = new();

		/// <summary>
		/// The endpoint that the RP Server is bound to.
		/// </summary>
		public readonly IPEndPoint endPoint;

		/// <summary>
		/// Creates a new API server, and binds it to the given endpoint.
		/// </summary>
		public RPServer(PrivateIdentity privateIdentity, IPEndPoint endPoint, ICallable callHandler)
		{
			listener = new SocketServer(endPoint);
			this.endPoint = endPoint;
			authenticator = new Authenticator(privateIdentity);
			this.callHandler = callHandler;
		}

		/// <summary>
		/// Repeatedly listens for connection requests until disposed.
		/// </summary>
		public async Task ListenForClients()
		{
			// Get possibly accepted listener.
			var accepted = listener.Accept();

			// Repeat until disposed.
			while (!disposed)
			{
				// Check for disposal at least every second.
				using var cts = new CancellationTokenSource();
				var timeout = Task.Delay(1000, cts.Token);
				await Task.WhenAny(accepted, timeout).ConfigureAwait(false);
				if (accepted.IsCompleted)
				{
					try
					{
						var socketMessenger = await accepted.ConfigureAwait(false);
						Handshake(socketMessenger);
					}
					catch { }

					try
					{
						// Always ensure to get accept a new connection,
						// regardless of whether the previous once failed.
						accepted = listener.Accept();
					}
					catch
					{
						break;
					}
				}
				cts.Cancel();
			}
		}

		/// <summary>
		/// Performs a handshake with a client.
		/// </summary>
		private async void Handshake(SocketMessenger socketMessenger)
		{
			// Begin process of connecting and upgrading
			ProtectedMessenger? protectedMessenger = null;
			AuthenticatedMessenger messenger;
			try
			{
				// Add connections to the HashSet for easy disposal signalling
				connections.Add(socketMessenger);

				protectedMessenger = await Protector.Protect(socketMessenger, false).ConfigureAwait(false);

				// Ensure one of the connections is in the HashSet at all time
				connections.Add(protectedMessenger);
				connections.Remove(socketMessenger);

				messenger = await authenticator.AuthenticateClient(protectedMessenger).ConfigureAwait(false);
			}
			catch
			{
				// Clean up any mess
				if (protectedMessenger is not null)
				{
					protectedMessenger.Dispose();
					connections.Remove(protectedMessenger);
				}
				else if (socketMessenger is not null)
				{
					socketMessenger.Dispose();
					connections.Remove(socketMessenger);
				}
				return;
			}
			// As the connection is complete, handle the client.
			ListenToClient(messenger);
		}

		/// <summary>
		/// Listens to client connections.
		/// </summary>
		private async void ListenToClient(AuthenticatedMessenger messenger)
		{
			try
			{
				while (true)
				{
					string received = await messenger.Receive().ConfigureAwait(false);
					HandleRequest(messenger, received);
				}
			}
			catch { }
			finally
			{
				// Ensure the messenger is disposed of at the end.
				messenger.Dispose();
				connections.Remove(messenger);
			}
		}

		/// <summary>
		/// Asynchronously handle a request.
		/// </summary>
		private async void HandleRequest(AuthenticatedMessenger messenger, string message)
		{
			RPCall call;
			try
			{
				var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };
				call = JsonConvert.DeserializeObject<RPCall>(message, settings) ?? throw new Exception();
			}
			catch
			{
				// Fatal exception
				messenger.Dispose();
				connections.Remove(messenger);
				return;
			}

			// Try calling the local function.
			string returnValues;
			bool success;
			try
			{
				returnValues = await callHandler.Call(call.Procedure, call.ProcedureArgs, messenger.otherIdentity).ConfigureAwait(false);
				success = true;
			}
			catch (RPException e)
			{
				returnValues = e.Message;
				success = false;
			}
			catch
			{
				returnValues = "Something went wrong!";
				success = false;
			}

			// Send the response.
			var response = new RPResponse { ID = call.ID, Success = success, ReturnValues = returnValues };
			try
			{
				string serialized = JsonConvert.SerializeObject(response);
				await messenger.Send(serialized).ConfigureAwait(false);
			}
			catch
			{
				if (connections.Contains(messenger))
				{
					messenger.Dispose();
					connections.Remove(messenger);
				}
				return;
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Dispose of listener and connections
				listener.Dispose();
				foreach (var connection in connections) connection.Dispose();
				connections.Clear();
			}

			disposed = true;
		}
	}
}