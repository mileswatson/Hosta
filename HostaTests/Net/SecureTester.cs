using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hosta.Crypto;
using Hosta.Net;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace HostaTests.Net
{
	[TestClass]
	public class SecureTester
	{
		public SecureMessenger a;
		public SecureMessenger b;

		public SecureTester()
		{
			using var server = new SecureServer(12000);

			SecureClient client = new SecureClient();

			var connected = client.Connect(server.Address, server.Port);

			a = server.Accept().Result;

			b = connected.Result;
		}

		[TestMethod]
		public void ConnectionSucceeded()
		{
			Assert.IsNotNull(a);
			Assert.IsNotNull(b);
		}

		[TestMethod]
		public void SharedKeySession()
		{
			var sharedKey = SecureRandomGenerator.GetBytes(100);

			using var server = new SecureServer(12000, sharedKey);

			SecureClient client = new SecureClient();

			var connected = client.Connect(server.Address, server.Port, sharedKey);

			Assert.IsNotNull(server.Accept().Result);
			Assert.IsNotNull(connected.Result);
		}

		[DataTestMethod]
		[DataRow(1)]
		[DataRow(563)]
		[DataRow((1 << 15) - 28)]
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
		[DataRow((1 << 15) - 27)]
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