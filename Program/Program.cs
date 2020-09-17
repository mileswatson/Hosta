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
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
			var listener = new SocketListener(11000);
			var a = listener.Start(cts.Token);

			Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			var ipAddress = ipHostInfo.AddressList[0];
			var remoteEndPoint = new IPEndPoint(ipAddress, 11000);
			s.Connect(remoteEndPoint);
			var client = new SocketMessenger(s);
			client.Send(new byte[] { 0, 1, 52 }).Wait();
			var handler = listener.Accept().Result;
			Console.WriteLine(string.Join(",", handler.Receive().Result.Select(o => o.ToString()).ToArray()));
			client.Dispose();
			handler.Dispose();
			a.Wait();
		}
	}
}