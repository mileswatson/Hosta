using System;
using System.Collections.Generic;

using Hosta.Net;
using Hosta.Crypto;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace Hosta.API
{
	/// <summary>
	/// A server that handles API requests.
	/// </summary>
	public class RPServer : IDisposable
	{
		/// <summary>
		/// Underlying Socket Server to listen for incoming
		/// connection requests.
		/// </summary>
		private readonly SocketServer listener;

		/// <summary>
		/// Protects an established connection with encryption.
		/// </summary>
		private readonly Protector protector = new Protector();

		/// <summary>
		/// Used for authenticating the client.
		/// </summary>
		private readonly Authenticator authenticator;

		/// <summary>
		/// Set of current connections.
		/// </summary>
		private readonly HashSet<IDisposable> connections
			= new HashSet<IDisposable>();

		/// <summary>
		/// The port that the server is bound to.
		/// </summary>
		public readonly IPEndPoint endPoint;

		/// <summary>
		/// Creates a new API server, and binds it to the given port.
		/// </summary>
		public RPServer(PrivateIdentity privateIdentity, IPEndPoint endPoint)
		{
			listener = new SocketServer(endPoint);
			this.endPoint = endPoint;
			authenticator = new Authenticator(privateIdentity);
		}

		/// <summary>
		/// Repeatedly listens for connection requests until disposed.
		/// </summary>
		public async Task Listen()
		{
			// Get possibly accepted listener.
			var accepted = listener.Accept();

			// Repeat until disposed.
			while (!disposed)
			{
				// Check for disposal at least every second.
				using var cts = new CancellationTokenSource();
				using var timeout = Task.Delay(1000, cts.Token);
				await Task.WhenAny(accepted, timeout);
				if (accepted.IsCompleted)
				{
					try
					{
						Handshake(await accepted);
					}
					finally
					{
						// Always ensure to get accept a new connection,
						// regardless of whether the previous once failed.
						accepted = listener.Accept();
					}
				}
				cts.Cancel();
			}
		}

		/// <summary>
		/// Performs a handshake with a client.
		/// </summary>
		public async void Handshake(SocketMessenger socketMessenger)
		{
			// Begin process of connecting and upgrading
			ProtectedMessenger protectedMessenger = null;
			AuthenticatedMessenger messenger;
			try
			{
				// Add connections to the HashSet for easy disposal signalling
				connections.Add(socketMessenger);

				ThrowIfDisposed();
				protectedMessenger = await protector.Protect(socketMessenger, false);

				// Ensure one of the connections is in the HashSet at all time
				connections.Add(protectedMessenger);
				connections.Remove(socketMessenger);

				ThrowIfDisposed();
				messenger = await authenticator.AuthenticateClient(protectedMessenger);
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
			Handle(messenger);
		}

		public static async void Handle(AuthenticatedMessenger messenger)
		{
			// Repeatedly echo client back.
			try
			{
				while (true)
				{
					string received = await messenger.Receive();
					await messenger.Send(received);
				}
			}
			catch { }
			finally
			{
				// Ensure the messenger is disposed of at the end.
				messenger.Dispose();
			}
		}

		public static IPAddress GetLocal()
		{
			return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
		}

		public static IPAddress GetExternal()
		{
			var externalip = new WebClient().DownloadString("http://icanhazip.com");
			return IPAddress.Parse(externalip.Trim());
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
			disposed = true;

			if (disposing)
			{
				// Dispose of managed resources

				listener.Dispose();
				foreach (var connection in connections) connection.Dispose();
			}
		}
	}
}