using Hosta.Crypto;
using Hosta.Net;
using Hosta.Tools;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Hosta
{
	public class Class
	{
		public static void Main()
		{
			using var socketServer = new SocketServer(11000);
			var accept = socketServer.Accept();

			var socketClient = new SocketClient();
			using SecureMessenger requester = new SecureMessenger(socketClient.Connect(socketServer.address, socketServer.port).Result, new byte[] { 1 }, new byte[] { 2 }, new byte[] { 3 });
			requester.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();
			requester.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();
			requester.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Wait();

			using SecureMessenger handler = new SecureMessenger(accept.Result, new byte[] { 2 }, new byte[] { 1 }, new byte[] { 3 });

			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));

			/*
			var start = new byte[] { 1 };
			AesCrypter aes = new AesCrypter(SecureRandom.GetBytes(32));
			var finish = aes.Decrypt(aes.Encrypt(start));
			Console.WriteLine(string.Join(",", finish.Select(o => o.ToString()).ToArray()));
			*/
		}
	}
}