using Hosta.Crypto;
using Hosta.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HostaTests.Net
{
	[TestClass]
	public class ConnectionTester
	{
		public SocketMessenger a;
		public SocketMessenger b;

		private Random r = new Random();

		public ConnectionTester()
		{
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);

			using var server = new SocketServer(serverEndpoint);

			var connected = SocketMessenger.CreateAndConnect(serverEndpoint);

			a = server.Accept().Result;

			b = connected.Result;
		}

		[DataTestMethod]
		[DataRow(1)]
		[DataRow(563)]
		[DataRow(1 << 16)]
		public async Task TestValid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			var sent = a.Send(bytes);
			var message = await b.Receive();
			Assert.IsTrue(await sent);
			Assert.IsTrue(Enumerable.SequenceEqual<byte>(bytes, message.Value));
		}

		[TestMethod]
		public async Task TestLoad()
		{
			HashSet<int> numbers = new();
			var iter = 1000;
			for (int i = 0; i < iter; i++)
			{
				Echo();
			}
			for (int i = 0; i < iter; i++)
			{
				var sent = a.Send(BitConverter.GetBytes(i));
				Assert.IsTrue(await sent);
				numbers.Add(i);
			}
			for (int i = 0; i < iter; i++)
			{
				var num = BitConverter.ToInt32((await a.Receive()).Value);
				var found = numbers.Remove(num);
				if (!found) throw new KeyNotFoundException();
			}
		}

		public async void Echo()
		{
			var received = await b.Receive();
			await Task.Delay(r.Next(10, 500));
			await b.Send(received.Value);
		}

		[DataTestMethod]
		[DataRow(0)]
		[DataRow((1 << 16) + 1)]
		public async Task TestInvalid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => a.Send(bytes));
		}

		[TestCleanup]
		public void CleanUp()
		{
			a.Dispose();
			b.Dispose();
		}
	}
}