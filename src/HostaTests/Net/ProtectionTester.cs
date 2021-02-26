using Hosta.Crypto;
using Hosta.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HostaTests.Net
{
	[TestClass]
	public class ProtectionTester
	{
		public ProtectedMessenger protected1;
		public ProtectedMessenger protected2;

		public ProtectionTester()
		{
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);

			using var server = new SocketServer(serverEndpoint);

			var connected = SocketMessenger.CreateAndConnect(serverEndpoint);

			var socket1 = server.Accept().Result.Value;
			var socket2 = connected.Result;

			var a = Protector.Protect(socket1, false);
			var b = Protector.Protect(socket2, true);

			protected1 = a.Result;
			protected2 = b.Result;
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
			var awaited = new List<Task<byte[]>>();
			for (int i = 0; i < iter; i++)
			{
				await protected1.Send(BitConverter.GetBytes(i));
				awaited.Add(protected1.Receive());
			}

			for (int i = 0; i < iter; i++)
			{
				Assert.AreEqual(BitConverter.ToInt32(await awaited[i]), i);
			}
		}

		public async void Echo(int iter)
		{
			Random r = new Random();
			for (int i = 0; i < iter; i++)
			{
				await Task.Delay(r.Next(0, 10));
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