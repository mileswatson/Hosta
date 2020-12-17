using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// An APM to TAP wrapper for the default socket class.
	/// </summary>
	public class SocketMessenger : IDisposable
	{
		/// <summary>
		/// The system socket to communicate with.
		/// </summary>
		private readonly Socket socket;

		/// <summary>
		/// Allows one message to be received at a time.
		/// </summary>
		private readonly AccessQueue readQueue = new AccessQueue();

		/// <summary>
		/// Allows one message to be sent at a time.
		/// </summary>
		private readonly AccessQueue writeQueue = new AccessQueue();

		/// <summary>
		/// The maximum length for a single message.
		/// </summary>
		private const int MaxLength = 1 << 15;

		/// <summary>
		/// Constructs a new SocketMessenger from a connected socket.
		/// </summary>
		public SocketMessenger(Socket connectedSocket)
		{
			socket = connectedSocket;
		}

		/// <summary>
		/// An APM to TAP wrapper for reading a message from the stream.
		/// </summary>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();

			// Enforce order.
			await readQueue.GetPass().ConfigureAwait(false);

			ThrowIfDisposed();
			try
			{
				// Read the length from the stream
				var lengthBytes = await ReadFixedLength(4).ConfigureAwait(false);

				// Convert the length to an integer and check that it's valid.
				int length = BitConverter.ToInt32(lengthBytes, 0);
				if (length <= 0 || length > MaxLength) throw new Exception("Message was an invalid length!");

				// Read the message from the stream
				return await ReadFixedLength(length).ConfigureAwait(false);
			}
			catch
			{
				// Any exception is fatal.
				Dispose();
				throw;
			}
			finally
			{
				readQueue.ReturnPass();
			}
		}

		/// <summary>
		/// A TAP to APM wrapper for reading a message from a stream.
		/// </summary>
		private Task<byte[]> ReadFixedLength(int length)
		{
			var tcs = new TaskCompletionSource<byte[]>();

			// Read data into fixed length buffer.
			byte[] messageBuffer = new byte[length];
			socket.BeginReceive(messageBuffer, 0, length, SocketFlags.None, ar =>
			{
				try
				{
					socket.EndReceive(ar);
					tcs.SetResult(messageBuffer);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);
			return tcs.Task;
		}

		/// <summary>
		/// Asynchronously sends a message over
		/// a TCP stream.
		/// </summary>
		public async Task Send(byte[] message)
		{
			ThrowIfDisposed();

			// Checks length before attempting to send.
			if (message.Length <= 0 || message.Length > MaxLength) throw new ArgumentOutOfRangeException(nameof(message));

			await writeQueue.GetPass().ConfigureAwait(false);

			try
			{
				await WriteLengthAndMessage(message).ConfigureAwait(false);
			}
			catch
			{
				// Any exception is fatal.
				Dispose();
				throw;
			}
			finally
			{
				writeQueue.ReturnPass();
			}
		}

		/// <summary>
		/// An TAP to APM wrapper for writing a message to a stream.
		/// </summary>
		private Task WriteLengthAndMessage(byte[] message)
		{
			var tcs = new TaskCompletionSource();

			// Concatenate length and message.
			List<byte> package = new List<byte>();
			package.AddRange(BitConverter.GetBytes(message.Length));
			package.AddRange(message);

			// Convert to an array and send.
			byte[] blob = package.ToArray();
			socket.BeginSend(blob, 0, blob.Length, 0, ar =>
			{
				try
				{
					socket.EndSend(ar);
					tcs.SetResult();
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);

			return tcs.Task;
		}

		/// <summary>
		/// Initiates a connection with a SocketServer.
		/// </summary>
		public static Task<SocketMessenger> CreateAndConnect(IPEndPoint serverEndpoint)
		{
			Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
			var tcs = new TaskCompletionSource<SocketMessenger>();

			s.BeginConnect(
				serverEndpoint,
				new AsyncCallback(ar =>
				{
					try
					{
						s.EndConnect(ar);
						tcs.SetResult(new SocketMessenger(s));
					}
					catch (Exception e)
					{
						tcs.SetException(e);
						s.Dispose();
					}
				}),
				null
			);
			return tcs.Task;
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
				// Dispose of waiting reads and writes.

				if (readQueue != null) readQueue.Dispose();
				if (writeQueue != null) writeQueue.Dispose();

				socket.Close();
			}

			disposed = true;
		}
	}
}