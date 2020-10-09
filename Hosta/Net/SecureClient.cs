using Hosta.Crypto;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class SecureClient
	{
		private SocketClient client = new SocketClient();

		public async Task<SecureMessenger> Connect(IPAddress address, int port, byte[] authKey = null)
		{
			var insecureMessenger = await client.Connect(address, port);
			byte[] key;
			if (authKey is null)
			{
				var exchanger = new KeyExchanger();
				var sent = insecureMessenger.Send(exchanger.Token);
				key = exchanger.KeyFromToken(await insecureMessenger.Receive());
				await sent;
			}
			else
			{
				key = authKey;
			}
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