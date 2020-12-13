using Hosta.Crypto;
using System;
using System.Threading.Tasks;

namespace Node
{
	internal class Program
	{
		private static Node node;

		public static async Task Main()
		{
			var serverID = new PrivateIdentity();

			Console.WriteLine(serverID.ID);

			node = new Node(serverID, Node.Binding.Loopback);

			var running = node.Run();

			Console.CancelKeyPress += new ConsoleCancelEventHandler(onCancel);

			Console.WriteLine("Running...");
			await running;
			Console.WriteLine("Done.");
		}

		protected static void onCancel(object sender, ConsoleCancelEventArgs args)
		{
			Console.WriteLine("\nHalting server...");
			node.Dispose();
			args.Cancel = true;
		}
	}
}