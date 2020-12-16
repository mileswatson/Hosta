using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class Authenticator
	{
		/// <summary>
		/// Used to sign data.
		/// </summary>
		private readonly PrivateIdentity self;

		/// <summary>
		/// Creates a new Authenticator from a Private Identity.
		/// </summary>
		public Authenticator(PrivateIdentity self) => this.self = self;

		/// <summary>
		/// Authenticates the connection with the client.
		/// </summary>
		public async Task<AuthenticatedMessenger> AuthenticateClient(ProtectedMessenger protectedMessenger)
		{
			// Throw if client has the wrong address.
			var wantedID = Transcoder.TextFromBytes(await protectedMessenger.Receive());
			if (wantedID != self.ID) throw new Exception("Wrong address!");

			// Different HMACs ensures client cannot echo back signature.
			var hmac = new HMACSHA256(protectedMessenger.ID);

			// Send the public key and the signature.
			await Task.WhenAll(
				protectedMessenger.Send(self.PublicIdentityInfo),
				protectedMessenger.Send(self.Sign(hmac.ComputeHash(new byte[] { 1 })))
			);

			// Gets the client public key.
			var pkInfo = await protectedMessenger.Receive();
			var clientIdentity = new PublicIdentity(pkInfo);

			// Verifies the client's signature.
			if (!clientIdentity.Verify(hmac.ComputeHash(new byte[] { 2 }), await protectedMessenger.Receive()))
				throw new Exception("Could not authenticate session!");

			// Returns an authenticated messenger.
			return new AuthenticatedMessenger(protectedMessenger, clientIdentity);
		}

		/// <summary>
		/// Authenticates a connection with the server.
		/// </summary>
		public async Task<AuthenticatedMessenger> AuthenticateServer(ProtectedMessenger protectedMessenger, string serverID)
		{
			// Initialise connection attempt.
			await protectedMessenger.Send(Transcoder.BytesFromText(serverID));

			// Receive the server's public key.
			var pkInfo = await protectedMessenger.Receive();
			PublicIdentity serverIdentity;
			serverIdentity = new PublicIdentity(pkInfo);

			// Checks that the server has the correct ID.
			if (serverIdentity.ID != serverID) throw new Exception("Wrong address!");

			// Different HMACs ensures client cannot echo back signature.
			var hmac = new HMACSHA256(protectedMessenger.ID);

			// Verifies the server's signature.
			var signature = await protectedMessenger.Receive();
			if (!serverIdentity.Verify(hmac.ComputeHash(new byte[] { 1 }), signature)) throw new Exception("Could not authenticate session!");

			// Send the public key and the signature.
			await Task.WhenAll(
				protectedMessenger.Send(self.PublicIdentityInfo),
				protectedMessenger.Send(self.Sign(hmac.ComputeHash(new byte[] { 2 })))
			);

			return new AuthenticatedMessenger(protectedMessenger, serverIdentity);
		}
	}
}