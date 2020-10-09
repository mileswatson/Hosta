using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Hosta.Crypto;

namespace Hosta.Net
{
	/// <summary>
	/// Used to send/receive messages over an encrypted session.
	/// </summary>
	public class SecureMessenger : IDisposable
	{
		private SocketMessenger socketMessenger;
		private SymmetricCrypter crypter;
		private KDFRatchet receiveRatchet;
		private KDFRatchet sendRatchet;
		private byte[] clicks;

		public SecureMessenger(SocketMessenger socketMessenger, byte[] sendKey = null, byte[] receiveKey = null, byte[] clicks = null)
		{
			this.socketMessenger = socketMessenger;
			crypter = new SymmetricCrypter();
			sendRatchet = new KDFRatchet(sendKey);
			receiveRatchet = new KDFRatchet(receiveKey);
			this.clicks = clicks;
		}

		public Task Send(byte[] message)
		{
			ThrowIfDisposed();
			var package = crypter.Encrypt(message, sendRatchet.Turn(clicks));
			return socketMessenger.Send(package);
		}

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