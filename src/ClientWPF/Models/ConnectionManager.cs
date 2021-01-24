using Hosta.API;
using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ClientWPF.Models
{
	/// <summary>
	/// Handles all the connections for the application.
	/// </summary>
	public class ConnectionManager : IDisposable
	{
		private readonly PrivateIdentity self;

		private readonly APITranslatorClient node;

		private readonly AsyncCache<APITranslatorClient> connections = new(
			// ensure that the item is not disposed
			(Task<APITranslatorClient> checking) => !(checking.IsCompletedSuccessfully && checking.Result.Disposed),

			// dispose of the item
			(Task<APITranslatorClient> disposing) =>
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
		public ConnectionManager(PrivateIdentity self, APITranslatorClient node, Action onConnectionFail)
		{
			this.self = self;
			this.node = node;
			connections.LazyGet(self.ID, () => Task.FromResult(node), TimeSpan.MaxValue, true);
			OnConnectionFail = onConnectionFail;
		}

		public Task<APITranslatorClient> GetConnection(string id, bool forceRefresh = false)
		{
			ThrowIfDisposed();
			// Attempt to fetch cached connection, otherwise query the node.
			return connections.LazyGet(id, async () =>
			{
				var response = await node.GetAddresses(new List<string> { id });
				try
				{
					var info = response[id];
					var conn = await APITranslatorClient.CreateAndConnect(new APITranslatorClient.ConnectionArgs
					{
						Address = IPAddress.Parse(info.IP),
						Port = info.Port,
						Self = self,
						ServerID = id
					});
					return conn;
				}
				catch
				{
					throw new Exception("Could not connect!");
				}
			}, TimeSpan.FromHours(1), forceRefresh);
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