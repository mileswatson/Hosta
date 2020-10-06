using Hosta.Net;
using Hosta.Tools;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Hosta
{
	public class Class
	{
		public static void Main()
		{
			using var socketServer = new SocketServer(11000);
			var accept = socketServer.Accept();

			var socketClient = new SocketClient();
			using SocketMessenger requester = socketClient.Connect(socketServer.Address, socketServer.Port).Result;
			requester.Send(new byte[] { 0, 1, 52 }).Wait();

			using SocketMessenger handler = accept.Result;

			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
		}
	}
}