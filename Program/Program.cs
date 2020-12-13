using Hosta.Crypto;
using Hosta.RPC;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Program
{
	public class Program
	{
		public static async Task Main()
		{
			var clientID = new PrivateIdentity();

			Console.Write("Server ID: ");
			var serverID = Console.ReadLine();

			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);

			var client = await RPClient.CreateAndConnect(serverID, serverEndpoint, clientID);

			Console.WriteLine("Calling...");
			Console.WriteLine(await client.Call("ValidProc", "tester"));

			client.Dispose();

			Console.ReadKey();
		}
	}
}