using Hosta.Crypto;
using Hosta.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
			using var server = new SocketServer(12000);

			SocketClient client = new SocketClient();

			var connected = client.Connect(server.address, server.port);

			a = server.Accept().Result;

			b = connected.Result;
		}

		[TestMethod]
		public void ConnectionSucceeded()
		{
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