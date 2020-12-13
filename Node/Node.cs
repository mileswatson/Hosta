using Hosta.Crypto;
using Hosta.RPC;
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
		/// Underlying server which handles RP requests.
		/// </summary>
		private RPServer server;

		/// <summary>
		/// Triggered when the object is disposed.
		/// </summary>
		private TaskCompletionSource onDisposed = new();

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
					address = IPAddress.Loopback;
					break;

				case Binding.Local:
					address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
					break;

				case Binding.Public:
					var externalip = new WebClient().DownloadString("http://icanhazip.com");
					address = IPAddress.Parse(externalip.Trim());
					break;

				default:
					address = IPAddress.None;
					break;
			}

			var serverEndpoint = new IPEndPoint(address, 12000);

			server = new RPServer(identity, serverEndpoint, Call);
		}

		/// <summary>
		/// Starts the server.
		/// </summary>
		public async Task Run()
		{
			var listening = server.ListenForClients();

			await listening;

			await onDisposed.Task;
		}

		/// <summary>
		/// Demo functionality.
		/// </summary>
		public async Task<string> Call(string proc, string args)
		{
			ThrowIfDisposed();
			Random r = new();
			await Task.Delay(r.Next(1000, 2000));
			if (proc == "ValidProc")
			{
				if (args == "InvalidArgs")
				{
					throw new Exception("InvalidArgsException");
				}
				else
				{
					return "RETURNVAL" + args;
				}
			}
			else
			{
				throw new Exception("InvalidProcedureException");
			}
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
				// Disposes of server, and then signals the main thread.
				server.Dispose();
				onDisposed.SetResult();
			}

			disposed = true;
		}
	}
}