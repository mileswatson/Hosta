using Hosta.Crypto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to establish an encrypted connection over an insecure connection.
	/// </summary>
	public class Protector
	{
		/// <summary>
		/// Key to initialise ratchets to. If null, a key exchange is performed.
		/// </summary>
		private readonly byte[] authKey;

		/// <summary>
		/// Creates a new instance of the Protector class.
		/// </summary>
		public Protector(byte[] authKey = null)
		{
			this.authKey = authKey;
		}

		/// <summary>
		/// Protects an insecure connection.
		/// </summary>
		/// <param name="initiator">Set to <see langword="true"/> if a client,
		/// <see langword="false"/> if a server.</param>
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

			// Send test data.
			var protectedMessenger = new ProtectedMessenger(socketMessenger, key, initiator);
			byte[] myValues = SecureRandomGenerator.GetBytes(32);
			var a = protectedMessenger.Send(myValues);

			// Echo bytes back.
			var b = protectedMessenger.Send(await protectedMessenger.Receive());
			await Task.WhenAll(a, b);

			// Check integrity of test data.
			if (!Enumerable.SequenceEqual<byte>(myValues, await protectedMessenger.Receive()))
				throw new Exception("Could not verify connection!");

			// Return protected messenger, as the connection has been validated.
			return protectedMessenger;
		}
	}
}