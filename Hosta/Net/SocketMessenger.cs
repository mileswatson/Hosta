using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// An APM to TAP wrapper for the default socket class.
	/// </summary>
	public class SocketMessenger : IDisposable
	{
		/// <summary>
		/// Controls logging.
		/// </summary>
		private readonly Logger logger;

		/// <summary>
		/// The system socket to communicate with.
		/// </summary>
		private readonly System.Net.Sockets.Socket socket;

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
		private const int MaxLength = 1000;

		/// <summary>
		/// Constructs a new SocketMessenger from a connected socket.
		/// </summary>
		/// <param name="connectedSocket">
		/// The underlying socket to use.
		/// </param>
		public SocketMessenger(System.Net.Sockets.Socket connectedSocket)
		{
			logger = new Logger(this);
			socket = connectedSocket;

			logger.Log("Construction finished.", VerbosityLevel.Standard);
		}

		/// <summary>
		/// An APM to TAP wrapper for reading a message from the stream.
		/// </summary>
		/// <returns>
		/// An awaitable task that resolves to the received blob.
		/// </returns>
		public async Task<byte[]> Receive()
		{
			ThrowIfDisposed();
			logger.Log("Awaiting read pass...", VerbosityLevel.Detailed);
			await readQueue.GetPass();
			ThrowIfDisposed();
			logger.Log("Read pass obtained.", VerbosityLevel.Detailed);
			try
			{
				var tcs = new TaskCompletionSource<byte[]>();
				ReadLength(tcs);
				return await tcs.Task;
			}
			catch (Exception e)
			{
				Dispose();
				logger.LogAndThrow(e, VerbosityLevel.Important);
				return null;
			}
			finally
			{
				readQueue.ReturnPass();
			}
		}

		/// <summary>
		/// Reads the length, then calls ReadMessage.
		/// </summary>
		/// <param name="tcs">TCS to pass to ReadMessage.</param>
		private void ReadLength(TaskCompletionSource<byte[]> tcs)
		{
			byte[] sizeBuffer = new byte[4];
			logger.Log("Beginning receive length...", VerbosityLevel.Detailed);
			socket.BeginReceive(sizeBuffer, 0, 4, 0, ar =>
			{
				try
				{
					socket.EndReceive(ar);
					int length = BitConverter.ToInt32(sizeBuffer, 0);
					logger.Log($"Received length {length}.", VerbosityLevel.Detailed);
					if (length > MaxLength) throw new Exception("Message was too long to receive!");
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
		/// <param name="tcs">TCS to set result of.</param>
		/// <param name="length">Length of message to read.</param>
		private void ReadMessage(TaskCompletionSource<byte[]> tcs, int length)
		{
			byte[] messageBuffer = new byte[length];
			logger.Log("Beginning receive message...", VerbosityLevel.Detailed);
			socket.BeginReceive(messageBuffer, 0, length, 0, ar =>
			{
				try
				{
					socket.EndReceive(ar);
					logger.Log("Received message.", VerbosityLevel.Detailed);
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
		/// <param name="message">The message to send.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public async Task Send(byte[] message)
		{
			ThrowIfDisposed();
			logger.Log("Awaiting write pass...", VerbosityLevel.Detailed);
			await writeQueue.GetPass();
			ThrowIfDisposed();
			logger.Log("Write pass obtained.", VerbosityLevel.Detailed);

			try
			{
				var tcs = new TaskCompletionSource<object>();
				if (message.Length > MaxLength) throw new Exception("Message is too large");
				WriteLengthAndMessage(tcs, message);
				await tcs.Task;
			}
			catch (Exception e)
			{
				Dispose();
				logger.LogAndThrow(e, VerbosityLevel.Important);
			}
			finally
			{
				writeQueue.ReturnPass();
			}
		}

		private void WriteLengthAndMessage(TaskCompletionSource<object> tcs, byte[] message)
		{
			List<byte> package = new List<byte>();
			package.AddRange(BitConverter.GetBytes(message.Length));
			package.AddRange(message);
			byte[] blob = package.ToArray();
			logger.Log("Beginning send...", VerbosityLevel.Detailed);
			socket.BeginSend(blob, 0, blob.Length, 0, ar =>
			{
				try
				{
					socket.EndSend(ar);
					tcs.SetResult(null);
					logger.Log("Send finished.", VerbosityLevel.Detailed);
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
			if (disposed) logger.LogAndThrow(new ObjectDisposedException("Attempted post-disposal use!"), VerbosityLevel.Standard);
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

				if (readQueue != null) readQueue.Dispose();
				if (writeQueue != null) writeQueue.Dispose();

				socket.Close();
				logger.Log("Socket has been closed.", VerbosityLevel.Detailed);

				logger.Log("Disposal finished.", VerbosityLevel.Important);
			}

			disposed = true;
		}
	}
}