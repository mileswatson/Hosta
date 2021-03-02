using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Used to perform an ECDH key exchange.
	/// </summary>
	public class KeyExchanger
	{
		private readonly ECDiffieHellman secret = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
		private readonly ECDiffieHellman foreignPublic = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);

		/// <summary>
		/// Gets the public key encoded as bytes.
		/// </summary>
		public byte[] Token => secret.ExportSubjectPublicKeyInfo();

		/// <summary>
		/// Decodes the foreign token into a public key, and generates the shared key.
		/// </summary>
		public byte[] KeyFromToken(byte[] foreignToken)
		{
			foreignPublic.ImportSubjectPublicKeyInfo(foreignToken, out _);
			return secret.DeriveKeyFromHash(foreignPublic.PublicKey, HashAlgorithmName.SHA256);
		}
	}
}