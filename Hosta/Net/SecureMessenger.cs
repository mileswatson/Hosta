using Hosta.Crypto;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to send/receive messages over an encrypted session.
	/// </summary>
	public class SecureMessenger : IDisposable
	{
		private readonly SocketMessenger socketMessenger;
		private readonly SymmetricCrypter crypter;
		private readonly KDFRatchet receiveRatchet;
		private readonly KDFRatchet sendRatchet;
		private readonly byte[] clicks;

		/// <summary>
		/// Creates a new instance of a SecureMessenger
		/// </summary>
		/// <param name="initiator">Indicates whether the socketMessenger initiated a connection or not.</param>
		public SecureMessenger(SocketMessenger socketMessenger, byte[] key, bool initiator)
		{
			this.socketMessenger = socketMessenger;
			crypter = new SymmetricCrypter();
			var hmac = new HMACSHA256(key);
			sendRatchet = new KDFRatchet(hmac.ComputeHash(initiator ? new byte[] { 1 } : new byte[] { 2 }));
			receiveRatchet = new KDFRatchet(hmac.ComputeHash(initiator ? new byte[] { 2 } : new byte[] { 1 }));
			this.clicks = hmac.ComputeHash(new byte[] { 3 });
		}

		/// <summary>
		/// Asynchronously sends an encrypted message.
		/// </summary>
		public Task Send(byte[] message)
		{
			ThrowIfDisposed();
			var package = crypter.Encrypt(message, sendRatchet.Turn(clicks));
			return socketMessenger.Send(package);
		}

		/// <summary>
		/// Asynchronously receives an encrypted message.
		/// </summary>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();
			var package = await socketMessenger.Receive();
			return crypter.Decrypt(package, receiveRatchet.Turn(clicks));
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
				socketMessenger.Dispose();
			}

			disposed = true;
		}
	}
}