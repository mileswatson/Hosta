using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using System;
using System.Threading.Tasks;

namespace ClientWPF.Models
{
	/// <summary>
	/// Handles network resources.
	/// </summary>
	public class ResourceManager : IDisposable
	{
		public string Self { get; init; }

		private readonly ConnectionManager connections;

		/// <summary>
		/// Creates a new instance of a ResourceManager.
		/// </summary>
		public ResourceManager(PrivateIdentity self, RemoteAPIGateway node, Action onConnectionFail)
		{
			Self = self.ID;
			connections = new ConnectionManager(self, node, () =>
			{
				Dispose();
				onConnectionFail();
			});
		}

		public async Task<GetProfileResponse> GetProfile(string user)
		{
			ThrowIfDisposed();
			var conn = await connections.GetConnection(user);
			return await conn.GetProfile();
		}

		//// Singleton

		public static ResourceManager? Resources { get; set; }

		//// Cleanup

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

			disposed = true;

			if (disposing)
			{
				connections.Dispose();
			}
		}
	}
}