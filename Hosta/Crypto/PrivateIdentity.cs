using System.Security.Cryptography;
using Hosta.Tools;

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
		private readonly ECDsa privateKey;

		/// <summary>
		/// Returns the PublicIdentityInfo
		/// </summary>
		public byte[] PublicIdentityInfo {
			get {
				return privateKey.ExportSubjectPublicKeyInfo();
			}
		}

		/// <summary>
		/// The ID of the PrivateIdentity.
		/// </summary>
		public readonly string ID;

		/// <summary>
		/// Generates a new private identity using the nistP521 curve.
		/// </summary>
		public PrivateIdentity()
		{
			privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP521);
			ID = PublicIdentity.IDFromPublicInformation(PublicIdentityInfo);
		}

		/// <summary>
		/// Signs data using the private key.
		/// </summary>
		public byte[] Sign(byte[] data)
		{
			return privateKey.SignData(data, HashAlgorithmName.SHA256);
		}
	}
}