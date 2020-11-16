﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

using Hosta.Crypto;
using Hosta.Net;

using Node;

namespace Program
{
	public class Class
	{
		public static void Main()
		{
			var serverID = new PrivateIdentity();
			var server = new APIServer(serverID, 12000);
			var listening = server.Listen();

			var client = new APIClient(new PrivateIdentity());

			var sw = new Stopwatch();
			sw.Start();

			var message = client.Communicate(serverID.ID,
				server.address,
				12000,
				"hello world"
				).Result;

			sw.Stop();
			Console.WriteLine(message);
			Console.WriteLine(sw.ElapsedMilliseconds);

			server.Dispose();
			listening.Wait();

			/*
			var tcs = new TaskCompletionSource();

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
			*/
		}
	}
}