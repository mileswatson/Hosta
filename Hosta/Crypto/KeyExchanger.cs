using System;
using System.Security.Cryptography;

namespace Hosta.Crypto
{
	/// <summary>
	/// Used to perform an ECDH key exchange.
	/// </summary>
	public class KeyExchanger : IDisposable
	{
		private readonly ECDiffieHellman secret = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
		private readonly ECDiffieHellman foreignPublic = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);

		/// <summary>
		/// Gets the public key encoded as bytes.
		/// </summary>
		public byte[] Token {
			get {
				ThrowIfDisposed();
				return secret.ExportSubjectPublicKeyInfo();
			}
		}

		/// <summary>
		/// Decodes the foreign token into a public key, and generates the shared key.
		/// </summary>
		public byte[] KeyFromToken(byte[] foreignToken)
		{
			ThrowIfDisposed();
			foreignPublic.ImportSubjectPublicKeyInfo(foreignToken, out _);
			return secret.DeriveKeyFromHash(foreignPublic.PublicKey, HashAlgorithmName.SHA256);
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("KeyExchanger has been disposed!");
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Disposed of keys.
				secret.Dispose();
				foreignPublic.Dispose();
			}

			disposed = true;
		}
	}
}