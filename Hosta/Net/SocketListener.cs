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
	public class SocketListener : IDisposable
	{
		// The underlying bound socket
		private Socket listener;

		/// <summary>
		/// Allows requests to be accepted before they are ready to be handled.
		/// </summary>
		private Queue<TaskCompletionSource<SocketMessenger>> accepted = new Queue<TaskCompletionSource<SocketMessenger>>();

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
		public SocketListener(int port)
		{
			// Get the IP of the local machine
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			var ipAddress = ipHostInfo.AddressList[0];

			Debug.Log($"Bound to address at {ipAddress.ToString()}");

			// Merge IP with port to create the local endpoint
			var localEndPoint = new IPEndPoint(ipAddress, port);

			// Create the listener
			listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Allow reuse of endpoint
			listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// Bind to endpoint
			listener.Bind(localEndPoint);
		}

		/// <summary>
		/// Returns an accepted connection as a SocketMessenger.
		/// </summary>
		/// <returns>The accepted SocketMessenger.</returns>
		public Task<SocketMessenger> Accept()
		{
			if (!listening) throw new Exception("Not listening!");
			if (waiting > 0 || accepted.Count == 0)
			{
				var tcs = new TaskCompletionSource<SocketMessenger>();
				accepted.Enqueue(tcs);
				waiting++;
				return tcs.Task;
			}
			return accepted.Dequeue().Task;
		}

		/// <summary>
		/// Starts the listening task.
		/// </summary>
		/// <param name="ct">Allows for cancellation of listening.</param>
		/// <returns>A task that completes when listening stops.</returns>
		public Task Start(CancellationToken ct)
		{
			// Ensure only one process runs at a time
			if (listening) throw new Exception("Already listening!");
			listening = true;

			// Create a task to run on a background thread
			return Task.Run(async () =>
			{
				// Allows for a backlog of 100 connections
				listener.Listen(100);

				// Process requests until cancelled
				while (!ct.IsCancellationRequested && !disposed)
				{
					try
					{
						// Resolves to connection
						Console.WriteLine("Waiting for a connection...");

						var tcs = new TaskCompletionSource<Socket>();
						var timeout = Task.Delay(5000);

						// Start the accept process
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

						// Regain control after connection or timeout.
						await Task.WhenAny(tcs.Task, timeout);

						// Break if no connection was made.
						if (!tcs.Task.IsCompleted)
						{
							break;
						}

						// Handle connection if accepted.
						HandleConnection(tcs.Task.Result);
					}
					catch (Exception e)
					{
						Debug.Log(e);
					}
				}
				if (listening) Dispose();
			});
		}

		/// <summary>
		/// Attempts to match a connection with a handler.
		/// </summary>
		/// <param name="socket"></param>
		private void HandleConnection(Socket socket)
		{
			ThrowIfDisposed();
			var messenger = new SocketMessenger(socket);
			if (waiting > 0)
			{
				accepted.Dequeue().SetResult(messenger);
			}
			var tcs = new TaskCompletionSource<SocketMessenger>();
			accepted.Enqueue(tcs);
			tcs.SetResult(messenger);
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
				if (waiting > 0)
				{
					while (accepted.Count > 0)
					{
						accepted.Dequeue().SetException(
							new ObjectDisposedException("AccessQueue has been disposed."));
					}
				}
				else
				{
					while (accepted.Count > 0)
					{
						accepted.Dequeue().Task.Result.Dispose();
					}
				}
				Debug.Log("SocketListener disposed.");
				disposed = true;
			}
		}
	}
}