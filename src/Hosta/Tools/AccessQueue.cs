using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hosta.Tools
{
	/// <summary>
	/// Used to asynchronously control access to a resource.
	/// </summary>
	public class AccessQueue : IDisposable
	{
		/// <summary>
		/// Enforces order of the queue.
		/// </summary>
		private readonly LinkedList<TaskCompletionSource> waitingTasks = new();

		private int available = 1;

		/// <summary>
		/// Queues for access to the resource.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		public Task GetPass()
		{
			ThrowIfDisposed();
			var tcs = new TaskCompletionSource();
			lock (waitingTasks)
			{
				waitingTasks.AddLast(tcs);
			}
			CheckForSpace();
			return tcs.Task;
		}

		/// <summary>
		/// Returns access to the resource.
		/// </summary>
		public void ReturnPass()
		{
			if (available == 1)
			{
				throw new SemaphoreFullException("All passes have been returned!");
			}
			available++;
			CheckForSpace();
		}

		/// <summary>
		/// Allocates any available passes.
		/// </summary>
		private void CheckForSpace()
		{
			lock (waitingTasks)
			{
				var currentNode = waitingTasks.First;
				while (currentNode != null && available > 0)
				{
					var tcs = currentNode.Value;
					available--;
					waitingTasks.Remove(currentNode);
					tcs.SetResult();
					currentNode = currentNode.Next;
				}
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
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				// Dispose of managed resources
				CheckForSpace();
				lock (waitingTasks)
				{
					var currentNode = waitingTasks.First;
					while (currentNode != null)
					{
						var tcs = currentNode.Value;
						waitingTasks.Remove(currentNode);
						tcs.SetException(
							new ObjectDisposedException("AccessQueue has been disposed!"));
						currentNode = currentNode.Next;
					}
				}
			}

			disposed = true;
		}
	}
}