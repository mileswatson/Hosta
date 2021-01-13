using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using System;
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

			using var client = await RemoteAPIGateway.CreateAndConnect(new RemoteAPIGateway.ConnectionArgs
			{
				Address = IPAddress.Loopback,
				Port = 12000,
				Self = clientID,
				ServerID = serverID
			});

			Console.WriteLine("Calling...");

			var hash = await client.AddBlob(new AddBlobRequest
			{
				Data = new byte[] { 1, 2, 3, 4, 1 }
			});

			Console.WriteLine(hash);

			Console.WriteLine((await client.GetBlob(hash)).Data.Length);

			Console.ReadKey();
		}
	}
}