using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Represents the private part of a public key pair.
	/// </summary>
	public class PrivateIdentity
	{
		/// <summary>
		/// Underlying ECDsa object to use.
		/// </summary>
		private ECDsa privateKey;

		/// <summary>
		/// Returns the PublicIdentityInfo
		/// </summary>
		public byte[] PublicIdentityInfo {
			get {
				return privateKey.ExportSubjectPublicKeyInfo();
			}
		}

		/// <summary>
		/// Generates a new private identity using the nistP521 curve.
		/// </summary>
		public PrivateIdentity()
		{
			privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP521);
		}

		/// <summary>
		/// Signs data using the private key.
		/// </summary>
		/// <param name="data">Data to sign.</param>
		/// <returns>Signature.</returns>
		public byte[] Sign(byte[] data)
		{
			return privateKey.SignData(data, HashAlgorithmName.SHA256);
		}
	}
}