using Hosta.Crypto;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// An server that spawns encrypted sessions.
	/// </summary>
	public class SecureServer : IDisposable
	{
		private readonly SocketServer server;
		private byte[] authKey;

		public int Port {
			get {
				ThrowIfDisposed();
				return server.port;
			}
		}

		public IPAddress Address {
			get {
				ThrowIfDisposed();
				return server.address;
			}
		}

		/// <summary>
		/// Creates a new instance of a SecureServer.
		/// </summary>
		/// <param name="authKey">The encryption key to use - if null, a key exchange is performed.</param>
		public SecureServer(int port, byte[] authKey = null)
		{
			this.authKey = authKey;
			server = new SocketServer(port);
		}

		/// <summary>
		/// Accepts and verifies a secure connection.
		/// </summary>
		public async Task<SecureMessenger> Accept()
		{
			ThrowIfDisposed();
			var insecureMessenger = await server.Accept();
			byte[] key = authKey;
			if (key is null)
			{
				var exchanger = new KeyExchanger();
				var sent = insecureMessenger.Send(exchanger.Token);
				key = exchanger.KeyFromToken(await insecureMessenger.Receive());
				await sent;
			}
			var secureMessenger = new SecureMessenger(insecureMessenger, key, false);
			byte[] myValues = SecureRandomGenerator.GetBytes(32);
			var a = secureMessenger.Send(myValues);
			var b = secureMessenger.Send(await secureMessenger.Receive());
			await Task.WhenAll(a, b);
			if (!Enumerable.SequenceEqual<byte>(myValues, await secureMessenger.Receive())) throw new Exception("Could not verify connection!");
			return secureMessenger;
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

				server.Dispose();

				disposed = true;
			}
		}
	}
}