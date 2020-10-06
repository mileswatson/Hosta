using Hosta.Tools;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Binds to a port and listens for incoming connection requests.
	/// </summary>
	public class SocketServer : IDisposable
	{
		private int port;

		public int Port {
			get { return port; }
		}

		private IPAddress address;

		public IPAddress Address {
			get { return address; }
		}

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
		public SocketServer(int port)
		{
			// Get the IP of the local machine
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			address = ipHostInfo.AddressList[0];
			this.port = port;

			// Merge IP with port to create the local endpoint
			var localEndPoint = new IPEndPoint(address, port);

			// Create the listener
			listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Allow reuse of endpoint
			listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// Bind to endpoint
			listener.Bind(localEndPoint);

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
							// End the accept process
							Socket connection = listener.EndAccept(ar);

							// Set the result
							tcs.SetResult(connection);
						}
						catch (Exception e)
						{
							tcs.SetException(e);
						}
					}),
					null
				);

				// Wait for finish, periodically checking for disposal
				var cts = new CancellationTokenSource();
				do
				{
					var timeout = Task.Delay(1000, cts.Token);
					await Task.WhenAny(tcs.Task, timeout);
					if (tcs.Task.IsCompleted)
					{
						break;
					}
					ThrowIfDisposed();
				} while (true);
				cts.Cancel();

				// Attempt to return the result
				try
				{
					var socket = tcs.Task.Result;
					return new SocketMessenger(tcs.Task.Result);
				}
				catch (Exception e)
				{
					return null;
				}
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
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Dispose of managed resources

				listener.Close();

				disposed = true;
			}
		}
	}
}