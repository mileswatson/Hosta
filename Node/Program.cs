using System;
using System.Threading.Tasks;
using Hosta.Crypto;

namespace Node
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var self = new PrivateIdentity();
			Console.WriteLine(self.ID);
			var server = new APIServer(self, 12000);
			Console.WriteLine(server.address);
			Task.Delay(20000).Wait();
		}
	}
}