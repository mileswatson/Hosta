using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Static class to allow cryptographically secure pseudorandom
	/// number generation (CSPRNG).
	/// </summary>
	public static class SecureRandomGenerator
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
	}
}