using Hosta.Crypto;
using Hosta.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.RPC
{
	/// <summary>
	/// Used to remotely call procedures on a server.
	/// </summary>
	public class RPClient : ICallable, IDisposable
	{
		/// <summary>
		/// Underlying messenger to use.
		/// </summary>
		private readonly AuthenticatedMessenger messenger;

		/// <summary>
		/// Keeps track of all unanswered requests.
		/// </summary>
		private readonly Dictionary<Guid, TaskCompletionSource<string>> awaitedResponses = new();

		/// <summary>
		/// Creates a new instance of an RPClient.
		/// </summary>
		private RPClient(AuthenticatedMessenger messenger)
		{
			this.messenger = messenger;
			ListenForResponses();
		}

		/// <summary>
		/// Creates an RPClient and connects it to an RPServer.
		/// </summary>
		public static async Task<RPClient> CreateAndConnect(string serverID, IPEndPoint serverEndpoint, PrivateIdentity self)
		{
			Authenticator authenticator = new Authenticator(self);

			// Begin process of connecting and upgrading
			SocketMessenger? socketMessenger = null;
			ProtectedMessenger? protectedMessenger = null;
			AuthenticatedMessenger messenger;
			try
			{
				socketMessenger = await SocketMessenger.CreateAndConnect(serverEndpoint).ConfigureAwait(false);
				protectedMessenger = await Protector.Protect(socketMessenger, true).ConfigureAwait(false);
				messenger = await authenticator.AuthenticateServer(protectedMessenger, serverID).ConfigureAwait(false);
			}
			catch
			{
				// If an exception is thrown, the highest level connection that is not null
				// must be disposed of.
				if (protectedMessenger is not null) protectedMessenger.Dispose();
				else if (socketMessenger is not null) socketMessenger.Dispose();

				// Signal that the connection was unsuccessful
				throw;
			}
			// As none of the connections failed, they must have succeeded.
			// Therefore, an RPClient is constructed and returned.
			return new RPClient(messenger);
		}

		/// <summary>
		/// Receives responses until disposed of.
		/// </summary>
		private async void ListenForResponses()
		{
			try
			{
				while (true)
				{
					string received = await messenger.Receive().ConfigureAwait(false);
					HandleResponse(received);
				}
			}
			catch
			{
			}
			finally
			{
				// Clean-up at the end
				Dispose();
			}
		}

		/// <summary>
		/// Tries to deserialize a response from an authenticated message,
		/// and then signals a waiting task if the response was obtained.
		/// </summary>
		private void HandleResponse(string received)
		{
			RPResponse response;
			try
			{
				var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };
				response = JsonConvert.DeserializeObject<RPResponse>(received, settings) ?? throw new Exception();
			}
			catch
			{
				// Any exception is fatal.
				return;
			}

			// Checks to see whether the request is still valid.
			if (!awaitedResponses.ContainsKey(response.ID)) return;

			var awaitedResponse = awaitedResponses[response.ID];

			awaitedResponses.Remove(response.ID);

			// Check if an exception was thrown or not.
			if (response.Success)
			{
				awaitedResponse.SetResult(response.ReturnValues);
			}
			else
			{
				awaitedResponse.SetException(new RPException(response.ReturnValues));
			}
		}

		/// <summary>
		/// Remotely calls a function on the other end and receives a response.
		/// </summary>
		public async Task<string> Call(string procedure, string args, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();

			// Construct call object.
			var call = new RPCall { ID = Guid.NewGuid(), Procedure = procedure, ProcedureArgs = args };

			// Allow for asynchronous return.
			var tcs = new TaskCompletionSource<string>();
			awaitedResponses.Add(call.ID, tcs);

			// Send the call object.
			await messenger.Send(JsonConvert.SerializeObject(call)).ConfigureAwait(false);

			// Await the response.
			return await tcs.Task.ConfigureAwait(false);
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("Attempted post-disposal use!");
		}

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
				// Dispose of authenticated messenger
				messenger.Dispose();

				// Signal awaited responses
				foreach (var kvp in awaitedResponses) kvp.Value.SetException(new ObjectDisposedException("RPClient has been disposed!"));
				awaitedResponses.Clear();
			}

			disposed = true;
		}
	}
}