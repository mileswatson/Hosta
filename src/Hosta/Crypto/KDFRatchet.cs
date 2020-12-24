using System;
using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// A HMACSHA512 ratchet.
	/// </summary>
	public class KDFRatchet
	{
		private byte[] key;

		/// <summary>
		/// Creates a new instance of a KDFRatchet.
		/// </summary>
		/// <param name="key">Key to start chain with.</param>
		public KDFRatchet(byte[]? key = null)
		{
			this.key = key ?? new byte[32];
		}

		/// <summary>
		/// Turns the ratchet, and returns the output.
		/// </summary>
		/// <param name="input">Data to feed to ratchet.</param>
		public byte[] Turn(byte[] input)
		{
			using var hmac = new HMACSHA512(key);
			key = hmac.ComputeHash(input);
			return (new ArraySegment<byte>(key, 32, 32)).ToArray();
		}
	}
}