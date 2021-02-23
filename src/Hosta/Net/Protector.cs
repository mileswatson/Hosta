using Hosta.Crypto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to establish an encrypted connection over an insecure connection.
	/// </summary>
	public static class Protector
	{
		/// <summary>
		/// Protects an insecure connection.
		/// </summary>
		/// <param name="initiator">Set to <see langword="true"/> if a client,
		/// <see langword="false"/> if a server.</param>
		public static async Task<ProtectedMessenger> Protect(SocketMessenger socketMessenger, bool initiator)
		{
			// Perform a key exchange.
			var exchanger = new KeyExchanger();
			var sent = socketMessenger.Send(exchanger.Token);
			var token = await socketMessenger.Receive().ConfigureAwait(false);
            if (token.IsError) throw new Exception(token.Error.GetType().ToString());
			var key = exchanger.KeyFromToken(token.Value);
            
			var result = await sent;
            if (result.IsError) throw new Exception(result.Error.GetType().ToString());

			// Send test data.
			var protectedMessenger = new ProtectedMessenger(socketMessenger, key, initiator);
			byte[] myValues = SecureRandomGenerator.GetBytes(32);
			var a = protectedMessenger.Send(myValues);

			// Echo bytes back.
			var integrityChallenge = await protectedMessenger.Receive().ConfigureAwait(false);
			var b = protectedMessenger.Send(integrityChallenge);
			await Task.WhenAll(a, b).ConfigureAwait(false);

			// Check integrity of test data.
			var integrityCheck = await protectedMessenger.Receive();
			if (!Enumerable.SequenceEqual<byte>(myValues, integrityCheck))
				throw new Exception("Could not verify connection!");

			// Return protected messenger, as the connection has been validated.
			return protectedMessenger;
		}
	}
}