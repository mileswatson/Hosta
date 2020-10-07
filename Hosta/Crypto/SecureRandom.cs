using System;
using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Static class to allow cryptographically secure pseudorandom
	/// number generation (CSPRNG).
	/// </summary>
	public static class SecureRandom
	{
		/// <summary>
		/// Generates cryptographically-secure random bytes.
		/// </summary>
		/// <param name="size">The number of bytes.</param>
		/// <returns>The generated random bytes.</returns>
		public static byte[] GetBytes(int size)
		{
			var rng = RandomNumberGenerator.Create();
			byte[] randombytes = new byte[size];
			rng.GetBytes(randombytes);
			return randombytes;
		}

		/// <summary>
		/// Generates a number x such that
		/// minimum <= x < maximum
		/// </summary>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <returns>The randomly generated number.</returns>
		public static int GetInt(int minimum, int maximum)
		{
			byte[] randombytes = GetBytes(4);
			int x = BitConverter.ToInt32(randombytes);
			x = x < 0 ? -x : x;
			return (x % maximum - minimum) + minimum;
		}
	}
}