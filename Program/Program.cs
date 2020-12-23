using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using Hosta.RPC;
using Newtonsoft.Json;
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
			var serverID = Console.ReadLine() ?? throw new NullReferenceException();

			var client = await RemoteAPIGateway.CreateAndConnect(new RemoteAPIGateway.ConnectionArgs
			{
				Address = IPAddress.Loopback,
				Port = 12000,
				Self = clientID,
				ServerID = serverID
			});

			Console.WriteLine("Calling...");

			Console.WriteLine(await client.GetProfile());

			await client.SetProfile(new("newdisplayname", "newtagline", "newbio", ""));

			Console.WriteLine(await client.GetProfile());

			Console.WriteLine("Done!");

			client.Dispose();

			Console.ReadKey();
		}
	}
}