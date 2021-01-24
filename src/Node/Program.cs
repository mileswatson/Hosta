using System;
using System.Threading.Tasks;

namespace Node
{
	internal class Program
	{
		private static Node? node;

		/// <summary>
		/// Program entrypoint.
		/// </summary>
		public static async Task Main(string[] args)
		{
			// Ensure that directory path and port are given.

			string path;
			try
			{
				path = args[0];
			}
			catch
			{
				throw new Exception("Directory path required!");
			}

			int port;
			try
			{
				port = int.Parse(args[1]);
			}
			catch
			{
				throw new Exception("Port required!");
			}

			// Create and run node.
			node = await Node.Create(path, Node.Binding.Local, port);
			var running = node.Run();

			// Capture cancel event
			Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancel);

			Console.WriteLine("Running...");
			await running;
			Console.WriteLine("Done.");
		}

		// Dispose of node on cancel event.
		protected static void OnCancel(object? _, ConsoleCancelEventArgs args)
		{
			Console.WriteLine("\nHalting server...");
			node!.Dispose();
			args.Cancel = true;
		}
	}
}