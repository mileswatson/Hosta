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
			using var secureServer = new SecureServer(11000);

			var secureClient = new SecureClient();
			var a = secureClient.Connect(secureServer.Address, secureServer.Port);
			var b = secureServer.Accept();

			using var client = a.Result;
			using var handler = b.Result;

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