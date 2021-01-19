using Hosta.API;
using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.IO;
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
		private readonly APITranslationServer gateway;

		/// <summary>
		/// Handles API requests.
		/// </summary>
		private readonly APIGateway databaseHandler;

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
		private Node(PrivateIdentity identity, IPEndPoint endpoint, APIGateway dataBaseHandler)
		{
			databaseHandler = dataBaseHandler;
			gateway = new APITranslationServer(identity, endpoint, databaseHandler);
		}

		/// <summary>
		/// Creates a new node, loading the database and server identity
		/// from the provided path.
		/// </summary>
		public static async Task<Node> Create(string folder, Binding binding)
		{
			if (!Directory.Exists(folder)) throw new Exception("Path is not a folder!");

			var privateIdentity = await LoadIdentity(folder);

			var address = await AddressFromBinding(binding);
			var port = 12000;

			var databaseHandler = await APIGateway.Create(Path.Combine(folder, "hostanode.db"), privateIdentity.ID);

			Console.WriteLine($"Creating node with location {privateIdentity.ID}:{address}:{port}");

			return new Node(privateIdentity, new IPEndPoint(address, port), databaseHandler);
		}

		/// <summary>
		/// Loads identity from folder.
		/// </summary>
		private static async Task<PrivateIdentity> LoadIdentity(string folder)
		{
			var file = Path.Combine(folder, "node.identity");
			// If file doesn't exist, create and save a new identity.
			if (!File.Exists(file))
			{
				var privateIdentity = PrivateIdentity.Create();
				var bytes = Transcoder.HexFromBytes(PrivateIdentity.Export(privateIdentity));
				await File.WriteAllTextAsync(file, bytes);
				Console.WriteLine($"No identity found at {file}, so identity {privateIdentity.ID} was created.");
				return privateIdentity;
			}

			// Otherwise, attempt to load the identity from the file.
			try
			{
				var hex = await File.ReadAllTextAsync(file);
				var privateIdentity = PrivateIdentity.Import(Transcoder.BytesFromHex(hex));
				Console.WriteLine($"Found identity {privateIdentity.ID} at {file}, loaded successfully.");
				return privateIdentity;
			}
			catch
			{
				Console.WriteLine($"Failed to load identity at {file}.");
				throw;
			}
		}

		/// <summary>
		/// Gets the IP address for each binding.
		/// </summary>
		private static async Task<IPAddress> AddressFromBinding(Binding binding)
		{
			switch (binding)
			{
				case Binding.Loopback:
					// Probably 127.0.0.1
					return IPAddress.Loopback;

				case Binding.Local:
					// The IP address of the device on the LAN
					var hostEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
					return hostEntry.AddressList[0];

				case Binding.Public:
					// Gets the public IP of the router
					var webclient = new WebClient();
					var externalip = await webclient.DownloadStringTaskAsync("http://icanhazip.com");
					return IPAddress.Parse(externalip.Trim());

				default:
					throw new Exception("Invalid binding!");
			}
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
				// Disposes of gateway and database.
				gateway.Dispose();
			}

			disposed = true;
		}
	}
}