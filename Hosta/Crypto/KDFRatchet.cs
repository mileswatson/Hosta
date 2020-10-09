using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Hosta.Crypto
{
	public class KDFRatchet
	{
		private byte[] key;

		public KDFRatchet(byte[] key = null)
		{
			this.key = key is null ? new byte[32] : key;
		}

		public byte[] Turn(byte[] input = null)
		{
			input = input is null ? new byte[32] : input;
			using var hmac = new HMACSHA512(key);
			key = hmac.ComputeHash(input);
			return (new ArraySegment<byte>(key, 32, 32)).ToArray();
		}
	}
}