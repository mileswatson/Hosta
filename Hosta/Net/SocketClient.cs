using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hosta.Net
{
	/// <summary>
	/// Allows asynchronous connection to a SocketServer.
	/// </summary>
	public class SocketClient
	{
		/// <summary>
		/// Initiates a connection with a SocketServer.
		/// </summary>
		public Task<SocketMessenger> Connect(IPAddress address, int port)
		{
			Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
			var tcs = new TaskCompletionSource<SocketMessenger>();

			s.BeginConnect(
				new IPEndPoint(address, port),
				new AsyncCallback(ar =>
				{
					try
					{
						s.EndConnect(ar);
						tcs.SetResult(new SocketMessenger(s));
					}
					catch (Exception e)
					{
						tcs.SetException(e);
						s.Dispose();
					}
				}),
				null
			);
			return tcs.Task;
		}
	}
}