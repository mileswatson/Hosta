﻿using Hosta.Tools;
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
			socket = connectedSocket;
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
				Dispose();
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
			socket.BeginReceive(sizeBuffer, 0, 4, 0, ar =>
			{
				try
				{
					socket.EndReceive(ar);
					int length = BitConverter.ToInt32(sizeBuffer, 0);
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
		/// <param name="message">The message to send.</param>
		/// <returns>
		/// An awaitable task.
		/// </returns>
		public async Task Send(byte[] message)
		{
			ThrowIfDisposed();
			await writeQueue.GetPass();
			ThrowIfDisposed();

			try
			{
				var tcs = new TaskCompletionSource<object>();
				if (message.Length > MaxLength) throw new Exception("Message is too large");
				WriteLengthAndMessage(tcs, message);
				await tcs.Task;
			}
			catch
			{
				Dispose();
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
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Dispose of managed resources

				if (readQueue != null) readQueue.Dispose();
				if (writeQueue != null) writeQueue.Dispose();

				socket.Close();
			}

			disposed = true;
		}
	}
}