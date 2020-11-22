using Hosta.RPC;
using Hosta.Crypto;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Program
{
	public class Class : ICallable
	{
		public static void Main()
		{
			var serverEndpoint = new IPEndPoint(RPServer.GetLocal(), 12000);

			var serverID = new PrivateIdentity();
			var server = new RPServer(serverID, serverEndpoint, new Class());
			var listening = server.ListenForClients();

			var client = RPClient.CreateAndConnect(serverID.ID, serverEndpoint, new PrivateIdentity()).Result;

			var sw = new Stopwatch();
			sw.Start();

			var calls = new List<Task<string>>();
			for (int i = 0; i < 1000; i++)
			{
				calls.Add(client.Call("this is call #", i.ToString()));
			}

			for (int i = 0; i < calls.Count; i++)
			{
				Console.WriteLine(calls[i].Result);
			}

			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds);

			server.Dispose();
			listening.Wait();
		}

		public Task<string> Call(string procedure, string args)
		{
			return Task<string>.FromResult(procedure + args);
		}
	}
}