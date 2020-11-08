using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class AuthenticatedMessenger : IDisposable
	{
		private readonly ProtectedMessenger protectedMessenger;

		public readonly PublicIdentity otherIdentity;

		public AuthenticatedMessenger(ProtectedMessenger secureMessenger, PublicIdentity otherIdentity)
		{
			this.protectedMessenger = secureMessenger;
			this.otherIdentity = otherIdentity;
		}

		public Task Send(string data)
		{
			ThrowIfDisposed();
			return protectedMessenger.Send(Transcoder.BytesFromText(data));
		}

		public async Task<string> Receive()
		{
			ThrowIfDisposed();
			var message = await protectedMessenger.Receive();
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
				protectedMessenger.Dispose();
			}

			disposed = true;
		}
	}
}