using Hosta.Crypto;
using Hosta.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.API
{
	/// <summary>
	/// Used to remotely call procedures on a server.
	/// </summary>
	public class RPClient : IDisposable
	{
		/// <summary>
		/// Protects an established connection with encryption.
		/// </summary>
		private readonly Protector protector = new Protector();

		/// <summary>
		/// Used for authenticating the server.
		/// </summary>
		private readonly Authenticator authenticator;

		/// <summary>
		/// Keeps a record of all the current connections, to allow for easy disposal.
		/// </summary>
		private readonly Dictionary<string, Task<AuthenticatedMessenger>> connections = new();

		/// <summary>
		/// Creates a new instance of an RPClient.
		/// </summary>
		public RPClient(PrivateIdentity privateIdentity)
		{
			authenticator = new Authenticator(privateIdentity);
		}

		/// <summary>
		/// Connects to an APIServer and performs a handshake.
		/// </summary>
		private async Task<AuthenticatedMessenger> ConnectAndHandshake(string serverID, IPEndPoint serverEndpoint)
		{
			ThrowIfDisposed();

			// Check if a connection is in progress / has been made
			if (connections.ContainsKey(serverID))
			{
				// If so, no need to start another connection
				return await connections[serverID];
			};

			// Used to signal any other connection requests.
			var tcs = new TaskCompletionSource<AuthenticatedMessenger>();
			connections[serverID] = tcs.Task;

			// Begin process of connecting and upgrading
			SocketMessenger socketMessenger = null;
			ProtectedMessenger protectedMessenger = null;
			AuthenticatedMessenger messenger;
			try
			{
				socketMessenger = await SocketMessenger.CreateAndConnect(serverEndpoint);
				protectedMessenger = await protector.Protect(socketMessenger, true);
				messenger = await authenticator.AuthenticateServer(protectedMessenger, serverID);
			}
			catch (Exception e)
			{
				// If an exception is thrown, the highest level connection that is not null
				// must be disposed of.
				if (protectedMessenger is not null) protectedMessenger.Dispose();
				else if (socketMessenger is not null) socketMessenger.Dispose();

				// Signal that the connection was unsuccessful
				tcs.SetException(e);
				throw;
			}
			// As none of the connections failed, they must have succeeded.
			// Therefore, the authenticated messenger is returned.
			tcs.SetResult(messenger);
			return messenger;
		}

		/// <summary>
		/// A quick test to check that the APIServer is responding correctly.
		/// </summary>
		public async Task<string> Communicate(string serverID, IPEndPoint serverEndpoint, string message)
		{
			var connection = await ConnectAndHandshake(serverID, serverEndpoint);
			var sent = connection.Send(message);
			var received = connection.Receive();
			await sent;
			return await received;
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
				// Dispose of connections
				foreach (var kvp in connections) kvp.Value.Dispose();
				connections.Clear();
			}

			disposed = true;
		}
	}
}