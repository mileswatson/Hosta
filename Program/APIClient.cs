using Hosta.Crypto;
using Hosta.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
	public class APIClient
	{
		private readonly PrivateIdentity self;

		private readonly SocketClient socketClient = new SocketClient();

		private readonly Protector protector = new Protector();

		private readonly Authenticator authenticator;

		private readonly Dictionary<string, Task<AuthenticatedMessenger>> connections =
			new Dictionary<string, Task<AuthenticatedMessenger>>();

		public APIClient(PrivateIdentity privateIdentity)
		{
			self = privateIdentity;
			authenticator = new Authenticator(privateIdentity);
		}

		private async Task<AuthenticatedMessenger> ConnectAndHandshake(string serverID, IPAddress address, int port)
		{
			if (connections.ContainsKey(serverID))
			{
				return await connections[serverID];
			};

			TaskCompletionSource<AuthenticatedMessenger> tcs = new TaskCompletionSource<AuthenticatedMessenger>();

			connections[serverID] = tcs.Task;

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
				if (protectedMessenger is not null) protectedMessenger.Dispose();
				else if (socketMessenger is not null) socketMessenger.Dispose();
				tcs.SetException(e);
				throw;
			}
			tcs.SetResult(messenger);
			return messenger;
		}

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