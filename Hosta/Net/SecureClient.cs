using Hosta.Crypto;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Allows asynchronous connection to a secure server.
	/// </summary>
	public class SecureClient
	{
		private readonly SocketClient client = new SocketClient();

		/// <summary>
		/// Connects to a secure server and verifies the connection.
		/// </summary>
		/// <param name="authKey">Key to use - if null, key exchange is performed.</param>
		public async Task<SecureMessenger> Connect(IPAddress address, int port, byte[] authKey = null)
		{
			// get socket connection
			var insecureMessenger = await client.Connect(address, port);

			byte[] key;
			if (authKey is null)
			{
				// perform key exchange
				var exchanger = new KeyExchanger();
				var sent = insecureMessenger.Send(exchanger.Token);
				key = exchanger.KeyFromToken(await insecureMessenger.Receive());
				await sent;
			}
			else
			{
				key = authKey;
			}

			// create a secure messenger and verify the connection
			var secureMessenger = new SecureMessenger(insecureMessenger, key, true);
			byte[] myValues = SecureRandomGenerator.GetBytes(32);
			var a = secureMessenger.Send(myValues);
			var b = secureMessenger.Send(await secureMessenger.Receive());
			await Task.WhenAll(a, b);
			if (!Enumerable.SequenceEqual<byte>(myValues, await secureMessenger.Receive())) throw new Exception("Could not verify connection!");

			return secureMessenger;
		}
	}
}