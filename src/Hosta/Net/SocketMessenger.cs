using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hosta.Tools;
using RustyResults;
using static RustyResults.Helpers;

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

		public IPEndPoint RemoteEndPoint => socket.RemoteEndPoint as IPEndPoint ?? throw new NullReferenceException();

		/// <summary>
		/// Constructs a new SocketMessenger from a connected socket.
		/// </summary>
		internal SocketMessenger(Socket connectedSocket)
		{
			socket = connectedSocket;
		}

		/// <summary>
		/// An APM to TAP wrapper for reading a message from the stream.
		/// </summary>
		public async Task<Result<byte[], ConnectionError, DisposedError>> Receive()
		{
			if (disposed) return Error(new DisposedError());

			// Enforce order.
			var pass = await readQueue.GetPass().ConfigureAwait(false);
			if (pass.IsError) return Error(new DisposedError());

			try
			{
				// Read the length from the stream
				var lengthBytes = await ReadUntilDone(4);
                
                if (lengthBytes.IsError)
                {
                    Dispose();
                    return Error(new ConnectionError());
                }

				// Convert the length to an integer and check that it's valid.
				int length = BitConverter.ToInt32(lengthBytes.Value, 0);
				if (length <= 0 || length > MaxLength) throw new Exception("Message was an invalid length!");

				// Read the message from the stream, and return it
				var message = await ReadUntilDone(length).ConfigureAwait(false);

                if (message.IsError)
                {
                    Dispose();
                    return Error(new ConnectionError());
                }

				return message.Value;
			}
			finally
			{
				readQueue.ReturnPass();
			}
		}

		/// <summary>
		/// Reads the given number of bytes into a buffer.
		/// </summary>
		private async Task<Result<byte[]>> ReadUntilDone(int length)
		{
			// Create a buffer to store the result
			var buffer = new byte[length];

			// Reading the full message may require multiple calls, so store
			// an offset to keep track of the number of bytes read.
			var offset = 0;
			while (offset < buffer.Length)
			{
				var result = await ReadIntoBuffer(buffer, offset).ConfigureAwait(false);
                if (result.IsError) return Error();

                var numBytes = result.Value;
				if (numBytes == 0) return Error();

				offset += numBytes;
			}

			// Return now that the buffer is full.
			return buffer;
		}

		/// <summary>
		/// A TAP to APM wrapper for reading a message from a stream.
		/// </summary>
		private Task<Result<int>> ReadIntoBuffer(byte[] buffer, int offset)
		{
            try
            {
                var tcs = new TaskCompletionSource<Result<int>>();

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
                        Debug.WriteLine(e);
                        tcs.SetResult(Error());
                    }
                }, null);
                return tcs.Task;
            }
			catch (Exception e)
            {
                Debug.WriteLine(e);
                return Task.FromResult<Result<int>>(Error());
            }
		}

        /// <summary>
        /// Asynchronously sends a message over a TCP stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the message length is <= 0 or >= MaxLength.</exception>
		public async Task<Status<ConnectionError, DisposedError>> Send(byte[] message)
		{
			if (disposed) return Error(new DisposedError());

			// Checks length before attempting to send.
			if (message.Length <= 0 || message.Length > MaxLength) throw new ArgumentOutOfRangeException(nameof(message));

			var pass = await writeQueue.GetPass().ConfigureAwait(false);
			if (pass.IsError) return Error(new DisposedError());

			try
			{
				// Convert the length of the message to bytes, then write it to the stream.
				var lengthBytes = BitConverter.GetBytes(message.Length);
				await WriteUntilDone(lengthBytes).ConfigureAwait(false);

				// Write the message to the stream
				await WriteUntilDone(message).ConfigureAwait(false);
                return Ok();
			}
			finally
			{
				writeQueue.ReturnPass();
			}
		}

		private async Task<Status> WriteUntilDone(byte[] buffer)
		{
			// Reading the full message may require multiple calls, so store
			// an offset to keep track of the number of bytes read.
			var offset = 0;
			while (offset < buffer.Length)
			{
				var numBytes = await WriteFromBuffer(buffer, offset).ConfigureAwait(false);
                if (numBytes.IsError) return Error();
				offset += numBytes.Value;
			}
            return Ok();
		}

		/// <summary>
		/// A TAP to APM wrapper for reading a message from a stream.
		/// </summary>
		private Task<Result<int>> WriteFromBuffer(byte[] buffer, int offset)
		{
            try 
            {
                var tcs = new TaskCompletionSource<Result<int>>();

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
                        Debug.WriteLine(e);
                        tcs.SetResult(Error());
                    }
                }, null);

                return tcs.Task;
            }
			catch (Exception e)
            {
                Debug.WriteLine(e);
                return Task.FromResult<Result<int>>(Error());
            }
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

        public struct DisposedError {}
        public struct ConnectionError {}

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