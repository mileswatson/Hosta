using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Represents the public part of private / public key pair.
	/// </summary>
	public class PublicIdentity
	{
		/// <summary>
		/// Underlying ECDsa object to use.
		/// </summary>
		private ECDsa publicKey = ECDsa.Create();

		/// <summary>
		/// Creates a new PublicIdentity from the given information.
		/// </summary>
		public PublicIdentity(byte[] publicIdentityInfo)
		{
			publicKey.ImportSubjectPublicKeyInfo(publicIdentityInfo, out _);
		}

		/// <summary>
		/// Checks if a signature is valid.
		/// </summary>
		/// <returns><see langword="true"/> if the signature is valid, else <see langword="false"/>.</returns>
		public bool Verify(byte[] data, byte[] signature)
		{
			return publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256);
		}
	}
}