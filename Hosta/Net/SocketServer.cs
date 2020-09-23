using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Hosta.Tools;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;

namespace Hosta.Net
{
	/// <summary>
	/// Binds to a port and listens for incoming connection requests.
	/// </summary>
	public class SocketServer : IDisposable
	{
		// The underlying bound socket
		private Socket listener;

		private AccessQueue acceptQueue = new AccessQueue();

		/// <summary>
		/// The number of empty TCS in the queue.
		/// </summary>
		private int waiting = 0;

		/// <summary>
		/// Whether the socket is listening.
		/// </summary>
		private bool listening = false;

		/// <summary>
		/// Constructs a socket listener bound to the given port.
		/// </summary>
		/// <param name="port">The port to bind to.</param>
		public SocketServer(int port)
		{
			// Get the IP of the local machine
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			var ipAddress = ipHostInfo.AddressList[0];

			//Logger.Log($"Bound to address at {ipAddress.ToString()}");

			// Merge IP with port to create the local endpoint
			var localEndPoint = new IPEndPoint(ipAddress, port);

			// Create the listener
			listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Allow reuse of endpoint
			listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// Bind to endpoint
			listener.Bind(localEndPoint);

			// Start listening
			listener.Listen(100);
			listening = true;
		}

		/// <summary>
		/// Returns an accepted connection as a SocketMessenger.
		/// </summary>
		/// <returns>The accepted SocketMessenger.</returns>
		public async Task<SocketMessenger> Accept()
		{
			if (!listening) throw new Exception("Not listening!");
			await acceptQueue.GetPass();
			if (!listening)
			{
				Dispose();
				throw new Exception("Not listening!");
			}
			var tcs = new TaskCompletionSource<Socket>();
			var timeout = Task.Delay(5000);
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
				await Task.WhenAny(tcs.Task, timeout);
				if (!tcs.Task.IsCompleted)
				{
					if (listening) Dispose();
					throw new TimeoutException("Socket machine broke");
				}
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
			if (disposed) throw new ObjectDisposedException("AccessQueue has been disposed!");
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
				listening = false;
				//Logger.Log("SocketListener disposed.");
				disposed = true;
			}
		}
	}
}