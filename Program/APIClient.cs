using Hosta.Crypto;
using Hosta.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Program
{
	public class APIClient
	{
		/// <summary>
		/// Used during the certificate creation process.
		/// </summary>
		private readonly PrivateIdentity self;

		/// <summary>
		/// Establishes a connection.
		/// </summary>
		private readonly SocketClient socketClient = new SocketClient();

		/// <summary>
		/// Protects an established connection with encryption.
		/// </summary>
		private readonly Protector protector = new Protector();

		/// <summary>
		/// Used for authenticating the server.
		/// </summary>
		private readonly Authenticator authenticator;

		/// <summary>
		///
		/// </summary>
		private readonly Dictionary<string, Task<AuthenticatedMessenger>> connections =
			new Dictionary<string, Task<AuthenticatedMessenger>>();

		/// <summary>
		/// Creates a new instance of an APIClient.
		/// </summary>
		public APIClient(PrivateIdentity privateIdentity)
		{
			self = privateIdentity;
			authenticator = new Authenticator(privateIdentity);
		}

		/// <summary>
		/// Connects to an APIServer and performs a handshake.
		/// </summary>
		/// <returns>An authenticated connection to the server.</returns>
		private async Task<AuthenticatedMessenger> ConnectAndHandshake(string serverID, IPAddress address, int port)
		{
			// Check if a connection is in progress / has been made
			if (connections.ContainsKey(serverID))
			{
				// If so, no need to start another connection
				return await connections[serverID];
			};

			// Used to signal any other connection requests.
			TaskCompletionSource<AuthenticatedMessenger> tcs = new TaskCompletionSource<AuthenticatedMessenger>();
			connections[serverID] = tcs.Task;

			// Begin process of connecting and upgrading
			SocketMessenger socketMessenger = null;
			ProtectedMessenger protectedMessenger = null;
			AuthenticatedMessenger messenger;
			try
			{
				socketMessenger = await socketClient.Connect(address, port);
				protectedMessenger = await protector.Protect(socketMessenger, true);
				messenger = await authenticator.AuthenticateServer(protectedMessenger, serverID);
			}
			catch (Exception e)
			{
				// If an exception is thrown, the greatest connection that is not null
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
		/// <returns>Hopefully the same string that was sent.</returns>
		public async Task<string> Communicate(string serverID, IPAddress address, int port, string message)
		{
			var connection = await ConnectAndHandshake(serverID, address, port);
			var sent = connection.Send(message);
			var received = connection.Receive();
			await sent;
			return await received;
		}
	}
}