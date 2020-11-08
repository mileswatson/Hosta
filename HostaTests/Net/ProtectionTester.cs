using Hosta.Crypto;
using Hosta.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HostaTests.Net
{
	[TestClass]
	public class ProtectionTester
	{
		public SocketMessenger socket1;
		public SocketMessenger socket2;

		public ProtectedMessenger protected1;
		public ProtectedMessenger protected2;

		public ProtectionTester()
		{
			using var server = new SocketServer(12000);

			var client = new SocketClient();
			var connected = client.Connect(server.address, server.port);

			socket1 = server.Accept().Result;
			socket2 = connected.Result;
		}

		[TestInitialize]
		[TestMethod]
		public async Task TestProtectorExchange()
		{
			byte[] key = null;
			var protector1 = new Protector(key);
			var protector2 = new Protector(key);

			var a = protector1.Protect(socket1, false);
			var b = protector2.Protect(socket2, true);

			protected1 = await a;
			protected2 = await b;
		}

		[TestMethod]
		[DataRow(0)]
		[DataRow(1)]
		[DataRow(100)]
		[DataRow(1000)]
		public async Task TestProtectorStatic(int length)
		{
			byte[] key = SecureRandomGenerator.GetBytes(length);

			using var server = new SocketServer(12000);

			var client = new SocketClient();

			var connected = client.Connect(server.address, server.port);

			var p1 = new Protector(key);
			var p2 = new Protector(key);

			var a = p1.Protect(await server.Accept(), false);
			var b = p1.Protect(await connected, true);

			await Task.WhenAll(a, b);
		}

		[DataTestMethod]
		[DataRow(1)]
		[DataRow(563)]
		[DataRow((1 << 15) - 28)]
		public async Task TestValid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			var sent = protected1.Send(bytes);
			var message = await protected2.Receive();
			await sent;
			Assert.IsTrue(Enumerable.SequenceEqual<byte>(bytes, message));
		}

		[DataTestMethod]
		[DataRow(0)]
		[DataRow((1 << 15) - 27)]
		public void TestInvalid(int length)
		{
			var bytes = SecureRandomGenerator.GetBytes(length);
			Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => protected1.Send(bytes));
		}

		[TestCleanup]
		public void CleanUp()
		{
			protected1.Dispose();
			protected2.Dispose();
		}
	}
}