using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hosta.Net;

namespace Hosta
{
	public class Class
	{
		public static void Main()
		{
			using var listener = new SocketServer(11000);
			var accept = listener.Accept();

			using Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			var ipAddress = ipHostInfo.AddressList[0];
			var remoteEndPoint = new IPEndPoint(ipAddress, 11000);
			s.Connect(remoteEndPoint);

			using SocketMessenger client = new SocketMessenger(s);
			client.Send(new byte[] { 0, 1, 52 }).Wait();

			using SocketMessenger handler = accept.Result;

			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
		}
	}
}