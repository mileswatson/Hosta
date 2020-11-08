using Hosta.Net;
using System;
using System.Linq;

namespace Hosta
{
	public class Class
	{
		public static void Main()
		{
			using var socketServer = new SocketServer(11000);

			var socketClient = new SocketClient();

			var a1 = socketClient.Connect(socketServer.address, socketServer.port);
			var b1 = socketServer.Accept();

			Protector protector = new Protector();

			var a2 = protector.Protect(a1.Result, true);
			var b2 = protector.Protect(b1.Result, false);

			var client = a2.Result;
			var handler = b2.Result;

			client.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();
			client.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();
			client.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();

			handler.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();
			handler.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();
			handler.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();

			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));

			Console.WriteLine(string.Join(",", client.Receive().Result.Select(o => o.ToString()).ToArray()));
			Console.WriteLine(string.Join(",", client.Receive().Result.Select(o => o.ToString()).ToArray()));
			Console.WriteLine(string.Join(",", client.Receive().Result.Select(o => o.ToString()).ToArray()));

			/*
			var start = new byte[] { 1 };
			AesCrypter aes = new AesCrypter(SecureRandom.GetBytes(32));
			var finish = aes.Decrypt(aes.Encrypt(start));
			Console.WriteLine(string.Join(",", finish.Select(o => o.ToString()).ToArray()));
			*/
		}
	}
}