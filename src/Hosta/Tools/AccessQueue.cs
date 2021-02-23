using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RustyResults;
using static RustyResults.Helpers;

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
		private readonly LinkedList<TaskCompletionSource<Status<DisposedError>>> waitingTasks = new();

		private bool available = true;

		/// <summary>
		/// Queues for access to the resource.
		/// </summary>
		/// <returns>An awaitable task.</returns>
		public async Task<Status<DisposedError>> GetPass()
		{
			if (disposed) return Error(new DisposedError());
			var tcs = new TaskCompletionSource<Status<DisposedError>>();
			lock (waitingTasks)
			{
				waitingTasks.AddLast(tcs);
			}
			CheckForSpace();
			return await tcs.Task;
		}

		/// <summary>
		/// Returns access to the resource.
		/// </summary>
		public void ReturnPass()
		{
			lock (waitingTasks)
			{
				if (available) throw new InvalidOperationException();
				available = true;
			}
			CheckForSpace();
		}

		/// <summary>
		/// Allocates any available passes.
		/// </summary>
		private void CheckForSpace()
		{
			lock (waitingTasks)
			{
				if (!available) return;
				var first = waitingTasks.First;
				if (first is null) return;
				available = false;
				waitingTasks.RemoveFirst();
				var tcs = first.Value;
				tcs.SetResult(Ok());
			}
		}

		public struct DisposedError { }

		//// Implements IDisposable

		private bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			disposed = true;

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
						tcs.SetResult(Error(new DisposedError()));
						currentNode = currentNode.Next;
					}
				}
			}
		}
	}
}