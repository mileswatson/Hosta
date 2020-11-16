using System;
using System.Collections.Generic;

using Hosta.Net;
using Hosta.Crypto;
using System.Threading.Tasks;
using System.Net;

namespace Node
{
	/// <summary>
	/// A server that handles API requests.
	/// </summary>
	public class APIServer : IDisposable
	{
		private readonly SocketServer listener;

		private readonly Protector protector = new Protector();
		private readonly Authenticator authenticator;

		private readonly HashSet<IDisposable> connections
			= new HashSet<IDisposable>();

		public readonly IPAddress address;

		public APIServer(PrivateIdentity privateIdentity, int port)
		{
			listener = new SocketServer(port);
			address = listener.address;
			authenticator = new Authenticator(privateIdentity);
		}

		public async Task Listen()
		{
			var accepted = listener.Accept();
			while (!disposed)
			{
				var timeout = Task.Delay(1000);
				await Task.WhenAny(accepted, timeout);
				if (accepted.IsCompleted)
				{
					try
					{
						Handshake(await accepted);
					}
					finally
					{
						accepted = listener.Accept();
					}
				}
			}
		}

		public async void Handshake(SocketMessenger socketMessenger)
		{
			ProtectedMessenger protectedMessenger = null;
			AuthenticatedMessenger messenger;

			try
			{
				connections.Add(socketMessenger);
				ThrowIfDisposed();
				protectedMessenger = await protector.Protect(socketMessenger, false);

				connections.Add(protectedMessenger);
				connections.Remove(socketMessenger);

				ThrowIfDisposed();
				messenger = await authenticator.AuthenticateClient(protectedMessenger);
			}
			catch
			{
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
			Handle(messenger);
		}

		public async void Handle(AuthenticatedMessenger messenger)
		{
			//string id = messenger.otherIdentity.ID;
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
				messenger.Dispose();
			}
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