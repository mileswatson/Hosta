using Hosta.API;
using Hosta.Crypto;
using System;
using System.Threading.Tasks;

namespace Node
{
	/// <summary>
	/// Handles API requests using an SQLite database.
	/// </summary>
	internal class DatabaseHandler : API, IDisposable
	{
		/// <summary>
		/// Demo functionality.
		/// </summary>
		public override async Task<string> Name(PublicIdentity _)
		{
			ThrowIfDisposed();
			var r = new Random();
			await Task.Delay(r.Next(10, 250));
			return "Firstname Lastname";
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
				// Dispose of database
			}

			disposed = true;
		}
	}
}