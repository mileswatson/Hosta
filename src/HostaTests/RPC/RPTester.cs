using Hosta.Crypto;
using Hosta.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace HostaTests.RPC
{
	[TestClass]
	public class RPTester : ICallable
	{
		private readonly Task listening;

		private readonly RPServer server;
		private readonly RPClient client;

		private const int iter = 1000;

		public RPTester()
		{
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12000);
			var serverID = PrivateIdentity.Create();
			server = new RPServer(serverID, serverEndpoint, this);
			listening = server.ListenForClients();

			var clientID = PrivateIdentity.Create();
			client = RPClient.CreateAndConnect(serverID.ID, serverEndpoint, clientID).Result;
		}

		[TestMethod]
		public async Task TestValid()
		{
			var tasks = new Task<string>[iter];
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
			var tasks = new Task<string>[iter];
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
				catch (RPException e)
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
			var tasks = new Task<string>[iter];
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
				catch (RPException e)
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

		public async Task<string> Call(string proc, string args, PublicIdentity? _1 = null, IPEndPoint? _2 = null)
		{
			Random r = new();
			await Task.Delay(r.Next(10, 200));
			if (proc == "ValidProc")
			{
				if (args.StartsWith("InvalidArgs"))
				{
					throw new RPException("InvalidArgsException");
				}
				else
				{
					return "RETURNVAL" + args;
				}
			}
			else
			{
				throw new RPException("InvalidProcedureException");
			}
		}
	}
}