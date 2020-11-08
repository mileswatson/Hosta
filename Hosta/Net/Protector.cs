using Hosta.Crypto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class Protector
	{
		private readonly byte[] authKey;

		public Protector(byte[] authKey = null)
		{
			this.authKey = authKey;
		}

		public async Task<ProtectedMessenger> Protect(SocketMessenger socketMessenger, bool initiator)
		{
			// If no static key is provided, perform a key exchange.
			byte[] key = authKey;
			if (key is null)
			{
				var exchanger = new KeyExchanger();
				var sent = socketMessenger.Send(exchanger.Token);
				key = exchanger.KeyFromToken(await socketMessenger.Receive());
				await sent;
			}

			// Send test data
			var protectedMessenger = new ProtectedMessenger(socketMessenger, key, initiator);
			byte[] myValues = SecureRandomGenerator.GetBytes(32);
			var a = protectedMessenger.Send(myValues);

			// Echo bytes back
			var b = protectedMessenger.Send(await protectedMessenger.Receive());
			await Task.WhenAll(a, b);

			// Check integrity of test data
			if (!Enumerable.SequenceEqual<byte>(myValues, await protectedMessenger.Receive())) throw new Exception("Could not verify connection!");

			// Return protected messenger
			return protectedMessenger;
		}
	}
}