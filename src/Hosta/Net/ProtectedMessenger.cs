using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Used to send/receive messages over an encrypted session.
	/// </summary>
	public class ProtectedMessenger : IDisposable
	{
		private readonly AccessQueue sendQueue = new();

		private readonly AccessQueue receiveQueue = new();

		/// <summary>
		/// Underlying Socket Messenger.
		/// </summary>
		private readonly SocketMessenger socketMessenger;

		/// <summary>
		/// Used to encrypt / decrypt messages.
		/// </summary>
		private readonly SymmetricCrypter crypter;

		/// <summary>
		/// Ticks over when a message is received.
		/// </summary>
		private readonly KDFRatchet receiveRatchet;

		/// <summary>
		/// Ticks over when a message is sent.
		/// </summary>
		private readonly KDFRatchet sendRatchet;

		/// <summary>
		/// The value that the ratchets are changed by.
		/// </summary>
		private readonly byte[] clicks;

		/// <summary>
		/// A unique conversation ID derived from the key.
		/// </summary>
		public readonly byte[] ID;

		public IPEndPoint RemoteEndPoint
		{
			get => socketMessenger.RemoteEndPoint;
		}

		/// <summary>
		/// Creates a new instance of a SecureMessenger
		/// </summary>
		/// <param name="initiator">Indicates whether the socketMessenger initiated a connection or not.</param>
		public ProtectedMessenger(SocketMessenger socketMessenger, byte[] key, bool initiator)
		{
			this.socketMessenger = socketMessenger;
			crypter = new SymmetricCrypter();
			var hmac = new HMACSHA256(key);

			// Chooses different keys depending on whether the connection was initiated,
			// so that the client and server can have opposite ratchets.
			sendRatchet = new KDFRatchet(hmac.ComputeHash(initiator ? new byte[] { 1 } : new byte[] { 2 }));
			receiveRatchet = new KDFRatchet(hmac.ComputeHash(initiator ? new byte[] { 2 } : new byte[] { 1 }));

			ID = hmac.ComputeHash(new byte[] { 3 });
			this.clicks = hmac.ComputeHash(new byte[] { 4 });
		}

		/// <summary>
		/// Asynchronously sends an encrypted message.
		/// </summary>
		public async Task Send(byte[] message)
		{
			ThrowIfDisposed();
			var pass = await sendQueue.GetPass().ConfigureAwait(false);
			if (pass.IsError) throw new Exception(pass.Error.GetType().ToString());
			try
			{
				var package = crypter.Encrypt(message, sendRatchet.Turn(clicks));
				var sent = await socketMessenger.Send(package).ConfigureAwait(false);
				if (sent.IsError) throw new Exception(sent.Error.GetType().ToString());
			}
			finally
			{
				sendQueue.ReturnPass();
			}
		}

		/// <summary>
		/// Asynchronously receives an encrypted message.
		/// </summary>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();
			var pass = await receiveQueue.GetPass().ConfigureAwait(false);
			if (pass.IsError) throw new Exception(pass.Error.GetType().ToString());
			try
			{
				var received = await socketMessenger.Receive().ConfigureAwait(false);
				if (received.IsError) throw new Exception(received.Error.GetType().ToString());

				var decryptResult = crypter.Decrypt(received.Value, receiveRatchet.Turn(clicks));
				if (decryptResult) return decryptResult.Value;
				else throw new Exception(decryptResult.Error.GetType().ToString());
			}
			finally
			{
				receiveQueue.ReturnPass();
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
				// Dispose of managed resources
				socketMessenger.Dispose();
			}

			disposed = true;
		}
	}
}