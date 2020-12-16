using Hosta.Crypto;
using Hosta.RPC;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.API
{
	/// <summary>
	/// Used to make requests to a remote API.
	/// </summary>
	public class RemoteAPIGateway : API, IDisposable
	{
		/// <summary>
		/// Underlying RP Client to use.
		/// </summary>
		private readonly RPClient client;

		/// <summary>
		/// Creates a new instance of a RemoteAPIGateway.
		/// </summary>
		private RemoteAPIGateway(RPClient client)
		{
			this.client = client;
		}

		/// <summary>
		/// Connects an RPClient to the given endpoint, and then constructs a RemoteAPIGateway from it.
		/// </summary>
		public static async Task<RemoteAPIGateway> CreateAndConnect(ConnectionArgs args)
		{
			var client = await RPClient.CreateAndConnect(
				args.ServerID,
				new IPEndPoint(args.Address, args.Port),
				args.Self ?? throw new NullReferenceException("Self should not be null!")
			);
			return new RemoteAPIGateway(client);
		}

		public override Task<string> Name(PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			return client.Call("Name", "");
		}

		public record ConnectionArgs
		{
			private string serverID = "";

			public string ServerID
			{
				get
				{
					return serverID; ;
				}
				init
				{
					serverID = value;
				}
			}

			private IPAddress address = IPAddress.None;

			public IPAddress Address
			{
				get
				{
					return address;
				}
				init
				{
					address = value;
				}
			}

			public int Port { get; init; }

			public PrivateIdentity? Self { get; init; }
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
				client.Dispose();
			}

			disposed = true;
		}
	}
}