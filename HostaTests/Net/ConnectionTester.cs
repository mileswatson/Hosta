using Hosta.Crypto;
using Hosta.Net;
using Hosta.RPC;
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

		[TestInitialize]
		public async Task ConnectionSucceeded()
		{
			var serverEndpoint = new IPEndPoint(RPServer.GetLocal(), 12000);

			using var server = new SocketServer(serverEndpoint);

			var connected = SocketMessenger.CreateAndConnect(serverEndpoint);

			a = await server.Accept();

			b = await connected;

			Assert.IsNotNull(a);
			Assert.IsNotNull(b);
		}

		[DataTestMethod]
		[DataRow(1)]
		[DataRow(563)]
		[DataRow(1 << 15)]
		public async Task TestValid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			var sent = a.Send(bytes);
			var message = await b.Receive();
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
				_ = a.Send(BitConverter.GetBytes(i));
			}
			for (int i = 0; i < iter; i++)
			{
				Assert.AreEqual(BitConverter.ToInt32(await a.Receive()), i);
			}
		}

		public async void Echo(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				_ = b.Send(await b.Receive());
			}
		}

		[DataTestMethod]
		[DataRow(0)]
		[DataRow((1 << 15) + 1)]
		public void TestInvalid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => a.Send(bytes));
		}

		[TestCleanup]
		public void CleanUp()
		{
			a.Dispose();
			b.Dispose();
		}
	}
}