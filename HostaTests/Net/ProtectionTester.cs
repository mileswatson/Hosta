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
	public class ProtectionTester
	{
		public SocketMessenger socket1;
		public SocketMessenger socket2;

		public ProtectedMessenger protected1;
		public ProtectedMessenger protected2;

		public ProtectionTester()
		{
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);

			using var server = new SocketServer(serverEndpoint);

			var connected = SocketMessenger.CreateAndConnect(serverEndpoint);

			socket1 = server.Accept().Result;
			socket2 = connected.Result;
		}

		[TestInitialize]
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
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);

			byte[] key = SecureRandomGenerator.GetBytes(length);

			using var server = new SocketServer(serverEndpoint);

			var connected = SocketMessenger.CreateAndConnect(serverEndpoint);

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

		[TestMethod]
		public async Task TestLoad()
		{
			var iter = 100;
			Echo(iter);
			Volley(iter);
			for (int i = 0; i < iter; i++)
			{
				Assert.AreEqual(BitConverter.ToInt32(await protected1.Receive()), i);
			}
		}

		public async void Volley(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				await Task.Delay(7);
				_ = protected1.Send(BitConverter.GetBytes(i));
			}
		}

		public async void Echo(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				await Task.Delay(13);
				_ = protected2.Send(await protected2.Receive());
			}
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