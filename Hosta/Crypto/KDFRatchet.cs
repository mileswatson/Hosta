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
		public KDFRatchet(byte[] key = null)
		{
			this.key = key is null ? new byte[32] : key;
		}

		/// <summary>
		/// Turns the ratchet, and returns the output.
		/// </summary>
		/// <param name="input">Optional data to feed to ratchet.</param>
		public byte[] Turn(byte[] input = null)
		{
			input = input is null ? new byte[32] : input;
			using var hmac = new HMACSHA512(key);
			key = hmac.ComputeHash(input);
			return (new ArraySegment<byte>(key, 32, 32)).ToArray();
		}
	}
}