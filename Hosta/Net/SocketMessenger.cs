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
			await readQueue.GetPass();

			ThrowIfDisposed();
			try
			{
				var tcs = new TaskCompletionSource<byte[]>();
				ReadLength(tcs);
				return await tcs.Task;
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
		/// Reads the length, then calls ReadMessage.
		/// </summary>
		private void ReadLength(TaskCompletionSource<byte[]> tcs)
		{
			byte[] sizeBuffer = new byte[4];
			socket.BeginReceive(sizeBuffer, 0, 4, 0, ar =>
			{
				try
				{
					socket.EndReceive(ar);

					// Reads length and checks it is valid.
					int length = BitConverter.ToInt32(sizeBuffer, 0);
					if (length <= 0 || length > MaxLength) throw new Exception("Message was an invalid length!");

					ReadMessage(tcs, length);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);
		}

		/// <summary>
		/// Reads the message, then returns it via the TCS.
		/// </summary>
		private void ReadMessage(TaskCompletionSource<byte[]> tcs, int length)
		{
			byte[] messageBuffer = new byte[length];
			socket.BeginReceive(messageBuffer, 0, length, 0, ar =>
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
		}

		/// <summary>
		/// An APM to TAP wrapper for writing
		/// bytes to the TCP stream.
		/// </summary>
		public async Task Send(byte[] message)
		{
			ThrowIfDisposed();

			// Checks length before attempting to send.
			if (message.Length <= 0 || message.Length > MaxLength) throw new ArgumentOutOfRangeException(nameof(message));

			await writeQueue.GetPass();
			ThrowIfDisposed();
			try
			{
				var tcs = new TaskCompletionSource<object>();
				WriteLengthAndMessage(tcs, message);
				await tcs.Task;
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

		private void WriteLengthAndMessage(TaskCompletionSource<object> tcs, byte[] message)
		{
			// Concatenate length and message
			List<byte> package = new List<byte>();
			package.AddRange(BitConverter.GetBytes(message.Length));
			package.AddRange(message);

			byte[] blob = package.ToArray();
			socket.BeginSend(blob, 0, blob.Length, 0, ar =>
			{
				try
				{
					socket.EndSend(ar);
					tcs.SetResult(null);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, null);
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