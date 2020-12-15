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
		private readonly ECDsa privateKey;

		/// <summary>
		/// Returns the PublicIdentityInfo
		/// </summary>
		public byte[] PublicIdentityInfo
		{
			get
			{
				return privateKey.ExportSubjectPublicKeyInfo();
			}
		}

		/// <summary>
		/// The ID of the PrivateIdentity.
		/// </summary>
		public readonly string ID;

		/// <summary>
		/// Generates a new private identity from a private key.
		/// </summary>
		private PrivateIdentity(ECDsa privateKey)
		{
			this.privateKey = privateKey;
			ID = PublicIdentity.IDFromPublicInformation(PublicIdentityInfo);
		}

		/// <summary>
		/// Signs data using the private key.
		/// </summary>
		public byte[] Sign(byte[] data)
		{
			return privateKey.SignData(data, HashAlgorithmName.SHA256);
		}

		/// <summary>
		/// Creates a new private identity from the nistP521 curve.
		/// </summary>
		public static PrivateIdentity Create()
		{
			var privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP521);
			return new PrivateIdentity(privateKey);
		}

		/// <summary>
		/// Imports a private identity from a byte array.
		/// </summary>
		public static PrivateIdentity Import(byte[] source)
		{
			var privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP521);
			privateKey.ImportECPrivateKey(source, out int _);
			return new PrivateIdentity(privateKey);
		}

		/// <summary>
		/// Exports a private identity as a byte array.
		/// </summary>
		public static byte[] Export(PrivateIdentity identity)
		{
			return identity.privateKey.ExportECPrivateKey();
		}
	}
}