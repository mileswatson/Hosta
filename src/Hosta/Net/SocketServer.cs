using Hosta.Tools;
using RustyResults;
using static RustyResults.Helpers;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

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
		public async Task<Result<SocketMessenger, ConnectionError, DisposedError>> Accept()
		{
			if (disposed) return Error(new DisposedError());

			var pass = await acceptQueue.GetPass().ConfigureAwait(false);
			if (pass.IsError) return Error(new DisposedError());

			var tcs = new TaskCompletionSource<Result<Socket, ConnectionError>>();
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
							Debug.WriteLine(e);
							tcs.SetResult(Error(new ConnectionError()));
						}
					}),
					null
				);

				// Check periodically for disposal
				do
				{
					var timeout = Task.Delay(1000);
					await Task.WhenAny(tcs.Task, timeout).ConfigureAwait(false);
					if (tcs.Task.IsCompleted)
					{
						break;
					}
					ThrowIfDisposed();
				} while (true);

				// Attempt to return the result
				var result = tcs.Task.Result;
				if (result.IsError) return Error(new ConnectionError());
				return new SocketMessenger(result.Value);
			}
			finally
			{
				acceptQueue.ReturnPass();
			}
		}

		public struct DisposedError { }
		public struct ConnectionError { }

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