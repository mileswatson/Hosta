using Hosta.API;
using Hosta.API.Image;
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

			using var client = await APITranslatorClient.CreateAndConnect(new APITranslatorClient.ConnectionArgs
			{
				Address = IPAddress.Loopback,
				Port = 12000,
				Self = clientID,
				ServerID = serverID
			});

			Console.WriteLine("Calling...");

			await client.AddImage(new AddImageRequest
			{
				Data = new byte[] { 1, 2, 3, 4, 0 }
			});

			await client.AddImage(new AddImageRequest
			{
				Data = new byte[] { 1, 2, 3, 4, 1 }
			});

			var infoList = await client.GetImageList();

			foreach (var info in infoList)
			{
				Console.WriteLine(info);
			}

			Console.WriteLine("Done.");

			Console.ReadKey();
		}
	}
}