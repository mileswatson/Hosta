using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class AuthenticatedMessenger : IDisposable
	{
		private readonly SecureMessenger secureMessenger;

		public readonly PublicIdentity otherIdentity;

		public AuthenticatedMessenger(SecureMessenger secureMessenger, PublicIdentity otherIdentity)
		{
			this.secureMessenger = secureMessenger;
			this.otherIdentity = otherIdentity;
		}

		public Task Send(string data)
		{
			ThrowIfDisposed();
			return secureMessenger.Send(Transcoder.BytesFromText(data));
		}

		public async Task<string> Receive()
		{
			ThrowIfDisposed();
			var message = await secureMessenger.Receive();
			try
			{
				return Transcoder.TextFromBytes(message);
			}
			catch
			{
				Dispose();
				throw new ObjectDisposedException("Attempted post-disposal use!");
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("Attempted post-disposal use!");
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
				// Dispose of managed resources
				secureMessenger.Dispose();
			}

			disposed = true;
		}
	}
}