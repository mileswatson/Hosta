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
		/// <summary>
		/// Underlying listener socket.
		/// </summary>
		private readonly Socket listener;

		/// <summary>
		/// Only one accept can happen at a time.
		/// </summary>
		private readonly AccessQueue acceptQueue = new AccessQueue();

		/// <summary>
		/// Controls logging.
		/// </summary>
		private readonly Logger logger;

		/// <summary>
		/// Constructs a socket listener bound to the given port.
		/// </summary>
		/// <param name="port">The port to bind to.</param>
		public SocketServer(int port)
		{
			// Initialise logger
			logger = new Logger(this);

			// Get the IP of the local machine
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			var ipAddress = ipHostInfo.AddressList[0];

			// Merge IP with port to create the local endpoint
			var localEndPoint = new IPEndPoint(ipAddress, port);

			// Create the listener
			listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Allow reuse of endpoint
			listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// Bind to endpoint
			listener.Bind(localEndPoint);
			logger.Log($"Listener has been bound to {localEndPoint}.", VerbosityLevel.Detailed);

			// Start listening
			listener.Listen(100);
			logger.Log("Listener is listening.", VerbosityLevel.Detailed);

			logger.Log("Construction finished.", VerbosityLevel.Important);
		}

		/// <summary>
		/// Returns an accepted connection as a SocketMessenger.
		/// </summary>
		/// <returns>The accepted SocketMessenger.</returns>
		public async Task<SocketMessenger> Accept()
		{
			ThrowIfDisposed();
			logger.Log("Waiting for an accept queue pass...", VerbosityLevel.Detailed);
			await acceptQueue.GetPass();
			logger.Log("Accept queue pass obtained.", VerbosityLevel.Detailed);
			ThrowIfDisposed();

			var tcs = new TaskCompletionSource<Socket>();
			try
			{
				logger.Log("Starting accept...", VerbosityLevel.Detailed);
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
					logger.Log("Connected successfully!", VerbosityLevel.Standard);
					return new SocketMessenger(tcs.Task.Result);
				}
				catch (Exception e)
				{
					logger.LogAndThrow(e, VerbosityLevel.Important);
					return null;
				}
			}
			finally
			{
				acceptQueue.ReturnPass();
				logger.Log("Accept queue pass returned.", VerbosityLevel.Detailed);
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) logger.LogAndThrow(new ObjectDisposedException("Attempted post-disposal use!"), VerbosityLevel.Important);
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

				logger.Log("Starting disposal...", VerbosityLevel.Standard);

				listener.Close();
				logger.Log("Listener has been closed.", VerbosityLevel.Detailed);

				disposed = true;
				logger.Log("Disposal finished.", VerbosityLevel.Important);
			}
		}
	}
}