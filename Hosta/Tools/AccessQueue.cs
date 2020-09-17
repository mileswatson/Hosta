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
		private readonly LinkedList<TaskCompletionSource<object>> waitingTasks
			= new LinkedList<TaskCompletionSource<object>>();

		private int available = 1;
		private readonly int maximum = 1;

		private bool checkingForSpace = false;

		/// <summary>
		/// Constructs a new AccessQueue.
		/// </summary>
		public AccessQueue()
		{
		}

		/// <summary>
		/// Queues for access to the resource.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		public Task GetPass()
		{
			ThrowIfDisposed();
			var tcs = new TaskCompletionSource<object>();
			waitingTasks.AddLast(tcs);
			CheckForSpace();
			return tcs.Task;
		}

		/// <summary>
		/// Returns access to the resource.
		/// </summary>
		public void ReturnPass()
		{
			if (available == maximum)
			{
				throw new SemaphoreFullException("All passes have been given returned!");
			}
			available++;
			CheckForSpace();
		}

		/// <summary>
		/// Task that checks to see if there's any
		/// </summary>
		private void CheckForSpace()
		{
			if (checkingForSpace) return;
			checkingForSpace = true;
			lock (waitingTasks)
			{
				var currentNode = waitingTasks.First;
				while (currentNode != null && available > 0)
				{
					var tcs = currentNode.Value;
					available--;
					waitingTasks.Remove(currentNode);
					tcs.SetResult(null);
					currentNode = currentNode.Next;
				}
				checkingForSpace = false;
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
				CheckForSpace();
				if (waitingTasks != null)
				{
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
			}

			disposed = true;
		}
	}
}