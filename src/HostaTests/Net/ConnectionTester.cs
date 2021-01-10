using Hosta.Crypto;
using Hosta.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
			var sent = a!.Send(bytes);
			var message = await b!.Receive();
			await sent;
			Assert.IsTrue(Enumerable.SequenceEqual<byte>(bytes, message));
		}

		[TestMethod]
		public async Task TestLoad()
		{
			var iter = 1000;
			Echo(iter);
			for (int i = 0; i < iter; i++)
			{
				_ = a!.Send(BitConverter.GetBytes(i));
			}
			for (int i = 0; i < iter; i++)
			{
				Assert.AreEqual(BitConverter.ToInt32(await a!.Receive()), i);
			}
		}

		public async void Echo(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				_ = b!.Send(await b.Receive());
			}
		}

		[DataTestMethod]
		[DataRow(0)]
		[DataRow((1 << 16) + 1)]
		public async Task TestInvalid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => a!.Send(bytes));
		}

		[TestCleanup]
		public void CleanUp()
		{
			a!.Dispose();
			b!.Dispose();
		}
	}
}