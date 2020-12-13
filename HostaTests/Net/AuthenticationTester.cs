using Hosta.Crypto;
using Hosta.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace HostaTests.Net
{
	[TestClass]
	public class AuthenticationTester
	{
		public SocketMessenger socket1;
		public SocketMessenger socket2;

		public ProtectedMessenger protected1;
		public ProtectedMessenger protected2;

		public AuthenticationTester()
		{
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);

			using var server = new SocketServer(serverEndpoint);

			var connected = SocketMessenger.CreateAndConnect(serverEndpoint);

			socket1 = server.Accept().Result;
			socket2 = connected.Result;

			var protector = new Protector();

			var a = protector.Protect(socket1, false);
			var b = protector.Protect(socket2, true);

			protected1 = a.Result;
			protected2 = b.Result;
		}

		[TestMethod]
		public async Task AuthenticateValid()
		{
			var serverPrivID = new PrivateIdentity();
			var serverPubID = new PublicIdentity(serverPrivID.PublicIdentityInfo);
			var clientPrivID = new PrivateIdentity();

			var serverAuthenticator = new Authenticator(serverPrivID);
			var clientAuthenticator = new Authenticator(clientPrivID);

			var a = serverAuthenticator.AuthenticateClient(protected1);
			var b = clientAuthenticator.AuthenticateServer(protected2, serverPubID.ID);

			await Task.WhenAll(a, b);
		}
	}
}