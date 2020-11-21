using Hosta.Tools;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Binds to a port and listens for incoming connection requests.
	/// </summary>
	public class SocketServer : IDisposable
	{
		public readonly int port;

		/// <summary>
		/// Underlying listener socket.
		/// </summary>
		private readonly Socket listener;

		/// <summary>
		/// Only one accept can happen at a time.
		/// </summary>
		private readonly AccessQueue acceptQueue = new AccessQueue();

		/// <summary>
		/// Constructs a socket listener bound to the given port.
		/// </summary>
		/// <param name="port">The port to bind to.</param>
		public SocketServer(IPEndPoint endPoint)
		{
			var address = endPoint.Address;

			// Create the listener
			listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Allow reuse of endpoint
			listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// Bind to endpoint
			listener.Bind(endPoint);

			// Start listening
			listener.Listen(100);
		}

		/// <summary>
		/// Accepts an incoming connection request.
		/// </summary>
		/// <returns>The accepted SocketMessenger.</returns>
		public async Task<SocketMessenger> Accept()
		{
			ThrowIfDisposed();

			await acceptQueue.GetPass();
			ThrowIfDisposed();

			var tcs = new TaskCompletionSource<Socket>();
			try
			{
				listener.BeginAccept(
					new AsyncCallback(ar =>
					{
						try
						{
							// End the accept process.
							Socket connection = listener.EndAccept(ar);

							// Set the result.
							tcs.SetResult(connection);
						}
						catch (Exception e)
						{
							tcs.SetException(e);
						}
					}),
					null
				);

				// Check periodically for disposal
				do
				{
					var timeout = Task.Delay(1000);
					await Task.WhenAny(tcs.Task, timeout);
					if (tcs.Task.IsCompleted)
					{
						break;
					}
					ThrowIfDisposed();
				} while (true);

				// Attempt to return the result
				var socket = tcs.Task.Result;
				return new SocketMessenger(tcs.Task.Result);
			}
			finally
			{
				acceptQueue.ReturnPass();
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
			disposed = true;

			if (disposing)
			{
				// Dispose of the listening socket and accept queue.
				acceptQueue.Dispose();
				listener.Dispose();
			}
		}
	}
}