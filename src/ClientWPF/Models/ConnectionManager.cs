using Hosta.API;
using Hosta.Crypto;
using System;
using System.Threading.Tasks;

namespace ClientWPF.Models
{
	/// <summary>
	/// Handles all the connections for the application.
	/// </summary>
	public class ConnectionManager : IDisposable
	{
		private readonly PrivateIdentity self;

		private readonly RemoteAPIGateway node;

		private readonly AsyncCache<RemoteAPIGateway> connections = new(
			// ensure that the item is not disposed
			(Task<RemoteAPIGateway> checking) => !(checking.IsCompletedSuccessfully && checking.Result.Disposed),

			// dispose of the item
			(Task<RemoteAPIGateway> disposing) =>
			{
				if (disposing.IsCompletedSuccessfully)
				{
					// if completed, dispose
					disposing.Result.Dispose();
				}
				else if (!disposing.IsCompleted)
				{
					// else, dispose when it completes
					disposing.ContinueWith(t => t.Result.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion);
				}
			}
		);

		private readonly Action OnConnectionFail;

		/// <summary>
		/// Create a new Connection Manager.
		/// </summary>
		public ConnectionManager(PrivateIdentity self, RemoteAPIGateway node, Action onConnectionFail)
		{
			this.self = self;
			this.node = node;
			connections.LazyGet(self.ID, () => Task.FromResult(node), TimeSpan.MaxValue, true);
			OnConnectionFail = onConnectionFail;
		}

		public Task<RemoteAPIGateway> GetConnection(string id, bool forceRefresh = false)
		{
			ThrowIfDisposed();
			// Throw an error if not cached - DNS not implemented
			return connections.LazyGet(id, () => Task.FromException<RemoteAPIGateway>(new NotImplementedException("IDAR has not been implemented!")), TimeSpan.FromHours(1), forceRefresh);
		}

		//// Disposal

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (node.Disposed) Dispose();
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
			disposed = true;

			if (disposing)
			{
				node.Dispose();
				var openConnections = connections.Dispose();
				foreach (var connection in openConnections)
				{
					if (connection.IsCompletedSuccessfully) connection.Result.Dispose();
				}
				OnConnectionFail();
			}
		}
	}
}