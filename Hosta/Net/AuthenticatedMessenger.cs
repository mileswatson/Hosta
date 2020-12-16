using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Represents an authenticated connection.
	/// </summary>
	public class AuthenticatedMessenger : IDisposable
	{
		/// <summary>
		/// Underlying Protected Messenger.
		/// </summary>
		private readonly ProtectedMessenger protectedMessenger;

		/// <summary>
		/// Identity of the correspondent.
		/// </summary>
		public readonly PublicIdentity otherIdentity;

		/// <summary>
		/// Creates a new authenticated messenger, given a protected messenger and a public identity.
		/// </summary>
		public AuthenticatedMessenger(ProtectedMessenger secureMessenger, PublicIdentity otherIdentity)
		{
			this.protectedMessenger = secureMessenger;
			this.otherIdentity = otherIdentity;
		}

		/// <summary>
		/// Converts the string to bytes, then sends it across the authenticated connection.
		/// </summary>
		public Task Send(string data)
		{
			ThrowIfDisposed();
			return protectedMessenger.Send(Transcoder.BytesFromText(data));
		}

		/// <summary>
		/// Receives bytes from an authenticated connection, then converts them to a string.
		/// </summary>
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
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Dispose of protected messenger
				protectedMessenger.Dispose();
			}

			disposed = true;
		}
	}
}