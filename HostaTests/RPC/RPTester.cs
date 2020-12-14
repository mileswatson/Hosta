using Hosta.Crypto;
using Hosta.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace HostaTests.RPC
{
	[TestClass]
	public class RPTester : ICallable
	{
		private Task listening;

		private RPServer server;
		private RPClient client;

		[TestInitialize]
		public async Task Connect()
		{
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);
			var serverID = new PrivateIdentity();
			server = new RPServer(serverID, serverEndpoint, this);
			listening = server.ListenForClients();

			var clientID = new PrivateIdentity();
			client = await RPClient.CreateAndConnect(serverID.ID, serverEndpoint, clientID);
		}

		[TestMethod]
		public async Task TestValid()
		{
			var tasks = new Task<string>[1000];
			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = client.Call("ValidProc", i.ToString());
			}
			for (int i = 0; i < tasks.Length; i++)
			{
				var val = await tasks[i];
				Assert.IsTrue(val == "RETURNVAL" + i.ToString());
			}
		}

		[TestMethod]
		public async Task TestInvalidArgs()
		{
			var tasks = new Task<string>[1000];
			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = client.Call("ValidProc", "InvalidArgs");
			}
			for (int i = 0; i < tasks.Length; i++)
			{
				bool thrown = false;
				try
				{
					var val = await tasks[i];
				}
				catch (Exception e)
				{
					thrown = true;
					Assert.IsTrue(e.Message == "InvalidArgsException");
				}
				Assert.IsTrue(thrown);
			}
		}

		[TestMethod]
		public async Task TestInvalidProc()
		{
			var tasks = new Task<string>[1000];
			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = client.Call("InvalidProc", i.ToString());
			}
			for (int i = 0; i < tasks.Length; i++)
			{
				bool thrown = false;
				try
				{
					var val = await tasks[i];
				}
				catch (Exception e)
				{
					thrown = true;
					Assert.IsTrue(e.Message == "InvalidProcedureException");
				}
				Assert.IsTrue(thrown);
			}
		}

		[TestCleanup]
		public async Task Cleanup()
		{
			server.Dispose();
			await listening;
			client.Dispose();
		}

		public async Task<string> Call(string proc, string args, PublicIdentity _ = null)
		{
			Random r = new();
			await Task.Delay(r.Next(10, 500));
			if (proc == "ValidProc")
			{
				if (args == "InvalidArgs")
				{
					throw new Exception("InvalidArgsException");
				}
				else
				{
					return "RETURNVAL" + args;
				}
			}
			else
			{
				throw new Exception("InvalidProcedureException");
			}
		}
	}
}