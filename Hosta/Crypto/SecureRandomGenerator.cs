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
		public static byte[] GetBytes(int length)
		{
			var rng = RandomNumberGenerator.Create();
			byte[] randombytes = new byte[length];
			rng.GetBytes(randombytes);
			return randombytes;
		}
	}
}