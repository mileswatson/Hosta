using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Hosta.Crypto
{
	/// <summary>
	/// Used to create and open encrypted packages.
	/// </summary>
	public class SymmetricCrypter
	{
		public const int KEY_SIZE = 32;
		public const int NONCE_SIZE = 12;
		public const int TAG_SIZE = 16;

		private byte[] key = new byte[KEY_SIZE];

		/// <summary>
		/// Sets the key for the AesCrypter
		/// </summary>
		public byte[] Key {
			set {
				if (value.Length != KEY_SIZE) throw new Exception("Key is not the correct size!");
				key = value;
			}
		}

		/// <summary>
		/// Creates a new AesCrypter.
		/// </summary>
		/// <param name="key">Default is an array of 0 bytes.</param>
		public SymmetricCrypter(byte[] key = null)
		{
			this.key = key is null ? new byte[KEY_SIZE] : key;
		}

		/// <summary>
		/// Encrypts data using AES-GCM, then packages with the authentication code and nonce.
		/// </summary>
		/// <param name="plainblob">The plainblob to encrypt.</param>
		/// <param name="overrideKey">An optional key to override the default key.</param>
		/// <returns>The secure package.</returns>
		public byte[] Encrypt(byte[] plainblob, byte[] overrideKey = null)
		{
			var nonce = SecureRandom.GetBytes(NONCE_SIZE);
			var cipherblob = new byte[plainblob.Length];
			var tag = new byte[TAG_SIZE];
			using var aes = new AesGcm(overrideKey is null ? key : overrideKey);
			aes.Encrypt(nonce, plainblob, cipherblob, tag);
			return nonce.Concat(tag).Concat(cipherblob).ToArray();
		}

		/// <summary>
		/// Decrypts data using AES-GCM, then packages it with the authentication code and nonce.
		/// </summary>
		/// <param name="package">The secure package to open.</param>
		/// <param name="overrideKey">An optional key to override the default key.</param>
		/// <returns></returns>
		public byte[] Decrypt(byte[] package, byte[] overrideKey = null)
		{
			var messageLength = package.Length - NONCE_SIZE - TAG_SIZE;
			if (messageLength < 0) throw new Exception("Package too small!");

			var nonce = new ArraySegment<byte>(package, 0, NONCE_SIZE);
			var tag = new ArraySegment<byte>(package, NONCE_SIZE, TAG_SIZE);
			var cipherblob = new ArraySegment<byte>(package, NONCE_SIZE + TAG_SIZE, messageLength);

			var plainblob = new byte[messageLength];

			try
			{
				using var aes = new AesGcm(overrideKey is null ? key : overrideKey);
				aes.Decrypt(nonce, cipherblob, tag, plainblob);
				return plainblob;
			}
			catch
			{
				throw new Exception("Message could not be decrypted!");
			}
		}
	}
}