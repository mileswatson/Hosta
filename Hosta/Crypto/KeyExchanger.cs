using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Hosta.Crypto
{
	public class KeyExchanger : IDisposable
	{
		private ECDiffieHellman secret = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
		private ECDiffieHellman foreignPublic = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);

		public byte[] Token {
			get {
				ThrowIfDisposed();
				return secret.ExportSubjectPublicKeyInfo();
			}
		}

		public void Reset()
		{
			secret = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
			disposed = false;
		}

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
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Disposed of managed resources
				secret.Dispose();
			}

			disposed = true;
		}
	}
}