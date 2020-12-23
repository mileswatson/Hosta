using Hosta.API.Data;
using Hosta.Crypto;
using Hosta.RPC;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.API
{
	/// <summary>
	/// Translates RPC calls to API calls.
	/// </summary>
	public class LocalAPIGateway : ICallable, IDisposable
	{
		/// <summary>
		/// Underlying API to call.
		/// </summary>
		private readonly API api;

		/// <summary>
		/// Underlying RPServer to receive calls from.
		/// </summary>
		private readonly RPServer server;

		/// <summary>
		/// Creates a new instance of a LocalAPIGateway.
		/// </summary>
		public LocalAPIGateway(PrivateIdentity self, IPEndPoint endPoint, API gateway)
		{
			server = new RPServer(self, endPoint, this);
			this.api = gateway;
		}

		/// <summary>
		/// Handles an RP call.
		/// </summary>
		public Task<string> Call(string proc, string args, PublicIdentity client)
		{
			if (client is null) throw new Exception("Unknown identity!");

			// Decides which handler to use.
			ProcedureHandler handler = proc switch
			{
				"GetProfile" => GetProfile,
				"SetProfile" => SetProfile,
				_ => throw new Exception("Invalid procedure!"),
			};

			return handler(args, client);
		}

		/// <summary>
		/// Starts the RPServer.
		/// </summary>
		public Task Run()
		{
			return server.ListenForClients();
		}

		/// <summary>
		/// Represents an RPC to API translator.
		/// </summary>
		private delegate Task<string> ProcedureHandler(string args, PublicIdentity client);

		//// Translators

		public async Task<string> GetProfile(string args, PublicIdentity client)
		{
			if (args != "") throw new Exception("Invalid Arguments!");
			var gpr = await api.GetProfile(client);
			return GetProfileResponse.Export(gpr);
		}

		public async Task<string> SetProfile(string args, PublicIdentity client)
		{
			await api.SetProfile(SetProfileRequest.Import(args), client);
			return "";
		}

		//// Implements IDisposable

		private bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				server.Dispose();
			}

			disposed = true;
		}
	}
}