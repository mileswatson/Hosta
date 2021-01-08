using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWPF.Models
{
	/// <summary>
	/// A simple thread-safe cache.
	/// </summary>
	public class AsyncCache<T>
	{
		/// <summary>
		/// Stores the keys and cached items.
		/// </summary>
		private readonly ConcurrentDictionary<string, CachedItem> memory = new();

		private readonly ItemChecker IsValidItem;

		private readonly ItemDisposer DisposeItem;

		/// <summary>
		/// Creates a new AsyncCache.
		/// </summary>
		/// <param name="itemChecker">Returns true if an item is valid, else false.</param>
		/// <param name="itemDisposer">Cleans up an item before removal.</param>
		public AsyncCache(ItemChecker itemChecker, ItemDisposer itemDisposer)
		{
			IsValidItem = itemChecker;
			DisposeItem = itemDisposer;
		}

		/// <summary>
		/// Tries to retrieve an item from cache - if not found it calls (and caches the output of) the factory.
		/// </summary>
		public Task<T> LazyGet(string key, Func<Task<T>> factory, TimeSpan lifetime, bool forceRefresh = false)
		{
			ThrowIfDisposed();

			// Trigger cleanup before attempting to fetch
			Cleanup();

			// Return item from cache if no forceRefresh, the item exists, and the item is valid
			if (
				!forceRefresh
				&& memory.TryGetValue(key, out CachedItem? x)
				&& IsValidItem(x.Task)
			)
			{
				return x.Task;
			}
			else
			{
				// otherwise, cleanup then produce a new item
				// the lifetime check allows an indefinite lifespan without an overflow
				Cleanup();
				var task = factory();
				var newItem = new CachedItem(
					task,
					lifetime == TimeSpan.MaxValue ? DateTimeOffset.MaxValue : DateTimeOffset.Now + lifetime
				);

				// add the item to the cache, disposing of the old item if it existed
				memory.AddOrUpdate(key, newItem, (string _, CachedItem oldItem) =>
				{
					DisposeItem(oldItem.Task);
					return newItem;
				});

				// return the task
				return task;
			}
		}

		/// <summary>
		/// Removes and disposes expired or invalid references.
		/// </summary>
		private void Cleanup()
		{
			var now = DateTimeOffset.Now;
			foreach (var kvp in memory)
			{
				if (kvp.Value.Expires < now || !IsValidItem(kvp.Value.Task))
				{
					memory.TryRemove(kvp.Key, out AsyncCache<T>.CachedItem? x);
					if (x is not null) DisposeItem(x.Task);
				}
			}
		}

		//// Delegates

		/// <summary>
		/// Returns true if the item is valid, otherwise false.
		/// </summary>
		public delegate bool ItemChecker(Task<T> checking);

		/// <summary>
		/// Disposes of an expired or invalid item.
		/// </summary>
		public delegate void ItemDisposer(Task<T> disposing);

		//// Cached Item

		private class CachedItem
		{
			public Task<T> Task { get; set; }
			public DateTimeOffset Expires { get; set; }

			public CachedItem(Task<T> item, DateTimeOffset expires)
			{
				Expires = expires;
				Task = item;
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("Attempted post-disposal use!");
		}

		public List<Task<T>> Dispose()
		{
			if (disposed) return new();
			disposed = true;
			var temp = memory.ToList();
			memory.Clear();
			return temp.Select(i => i.Value.Task).ToList();
		}
	}
}