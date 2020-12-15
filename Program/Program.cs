using Hosta.API;
using Hosta.Crypto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Program
{
	public class Program
	{
		public static async Task Main()
		{
			var clientID = PrivateIdentity.Create();

			Console.Write("Server ID: ");
			var serverID = Console.ReadLine();

			var client = await RemoteAPIGateway.CreateAndConnect(new RemoteAPIGateway.ConnectionArgs
			{
				Address = IPAddress.Loopback,
				Port = 12000,
				Self = clientID,
				ServerID = serverID
			});

			const int numCalls = 1000;

			Console.WriteLine("Calling...");
			var responses = new List<Task<string>>();

			for (int i = 0; i < numCalls; i++)
			{
				responses.Add(client.Name());
			}

			for (int i = 0; i < numCalls; i++)
			{
				Console.WriteLine(await responses[i]);
			}

			Console.WriteLine("Done!");

			client.Dispose();

			Console.ReadKey();
		}
	}
}