using Hosta.Tools;
using System;
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
		private const int MaxLength = 1 << 16;

		private const int ChunkSize = 8000;

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
				var lengthBytes = await ReadUntilDone(4);

				// Convert the length to an integer and check that it's valid.
				int length = BitConverter.ToInt32(lengthBytes, 0);
				if (length <= 0 || length > MaxLength) throw new Exception("Message was an invalid length!");

				// Read the message from the stream, and return it
				var message = await ReadUntilDone(length).ConfigureAwait(false);

				return message;
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
		/// Reads the given number of bytes into a buffer.
		/// </summary>
		private async Task<byte[]> ReadUntilDone(int length)
		{
			// Create a buffer to store the result
			var buffer = new byte[length];

			// Reading the full message may require multiple calls, so store
			// an offset to keep track of the number of bytes read.
			var offset = 0;
			while (offset < buffer.Length)
			{
				var numBytes = await ReadIntoBuffer(buffer, offset).ConfigureAwait(false);
				offset += numBytes;
			}

			// Return now that the buffer is full.
			return buffer;
		}

		/// <summary>
		/// A TAP to APM wrapper for reading a message from a stream.
		/// </summary>
		private Task<int> ReadIntoBuffer(byte[] buffer, int offset)
		{
			var tcs = new TaskCompletionSource<int>();

			// Read data into fixed length buffer.
			socket.BeginReceive(buffer, offset, buffer.Length - offset, SocketFlags.None, ar =>
			{
				try
				{
					var numBytesRead = socket.EndReceive(ar);
					tcs.SetResult(numBytesRead);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);
			return tcs.Task;
		}

		/// <summary>
		/// Asynchronously sends a message over a TCP stream.
		/// </summary>
		public async Task Send(byte[] message)
		{
			ThrowIfDisposed();

			// Checks length before attempting to send.
			if (message.Length <= 0 || message.Length > MaxLength) throw new ArgumentOutOfRangeException(nameof(message));

			await writeQueue.GetPass().ConfigureAwait(false);

			try
			{
				// Convert the length of the message to bytes, then write it to the stream.
				var lengthBytes = BitConverter.GetBytes(message.Length);
				await WriteUntilDone(lengthBytes).ConfigureAwait(false);

				// Write the message to the stream
				await WriteUntilDone(message).ConfigureAwait(false);
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

		private async Task WriteUntilDone(byte[] buffer)
		{
			// Reading the full message may require multiple calls, so store
			// an offset to keep track of the number of bytes read.
			var offset = 0;
			while (offset < buffer.Length)
			{
				var numBytes = await WriteFromBuffer(buffer, offset).ConfigureAwait(false);
				offset += numBytes;
			}
		}

		/// <summary>
		/// A TAP to APM wrapper for reading a message from a stream.
		/// </summary>
		private Task<int> WriteFromBuffer(byte[] buffer, int offset)
		{
			var tcs = new TaskCompletionSource<int>();

			// Read data into fixed length buffer.
			socket.BeginSend(buffer, offset, buffer.Length - offset, SocketFlags.None, ar =>
			{
				try
				{
					var numBytesWritten = socket.EndSend(ar);
					tcs.SetResult(numBytesWritten);
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

				socket.Dispose();
			}

			disposed = true;
		}
	}
}