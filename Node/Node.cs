using Hosta.Crypto;
using Hosta.API;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Node
{
	/// <summary>
	/// Represents a network node.
	/// </summary>
	internal class Node : IDisposable
	{
		/// <summary>
		/// Gateway to forward API requests.
		/// </summary>
		private readonly LocalAPIGateway gateway;

		/// <summary>
		/// Handles API requests.
		/// </summary>
		private readonly DatabaseHandler databaseHandler;

		/// <summary>
		/// IP binding modes for the node.
		/// </summary>
		public enum Binding
		{
			Loopback,
			Local,
			Public
		}

		/// <summary>
		/// Creates a new instance of a Node.
		/// </summary>
		public Node(PrivateIdentity identity, Binding binding)
		{
			IPAddress address;

			switch (binding)
			{
				case Binding.Loopback:
					// Probably 127.0.0.1
					address = IPAddress.Loopback;
					break;

				case Binding.Local:
					// The IP address of the device on the LAN
					address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
					break;

				case Binding.Public:
					// Gets the public IP of the router
					var externalip = new WebClient().DownloadString("http://icanhazip.com");
					address = IPAddress.Parse(externalip.Trim());
					break;

				default:
					throw new Exception("Invalid binding!");
			}

			var serverEndpoint = new IPEndPoint(address, 12000);

			databaseHandler = new DatabaseHandler();

			gateway = new LocalAPIGateway(identity, serverEndpoint, databaseHandler);
		}

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		public Task Run()
		{
			return gateway.Run();
		}

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

			if (disposing)
			{
				// Disposes of gateway and closes database.
				gateway.Dispose();
				databaseHandler.Dispose();
			}

			disposed = true;
		}
	}
}