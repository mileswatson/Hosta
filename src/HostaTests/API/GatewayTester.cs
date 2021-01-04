using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace HostaTests.API
{
	[TestClass]
	public class GatewayTester
	{
		private readonly PrivateIdentity server = PrivateIdentity.Create();
		private readonly PrivateIdentity client = PrivateIdentity.Create();

		private readonly Hosta.API.API api = new MockAPI();

		private readonly LocalAPIGateway localGateway;

		private readonly Task running;

		private readonly RemoteAPIGateway remoteGateway;

		public GatewayTester()
		{
			localGateway = new LocalAPIGateway(server, new IPEndPoint(IPAddress.Loopback, 12000), api);
			running = localGateway.Run();
			var args = new RemoteAPIGateway.ConnectionArgs { Address = IPAddress.Loopback, Port = 12000, Self = client, ServerID = server.ID };
			remoteGateway = RemoteAPIGateway.CreateAndConnect(args).Result;
		}

		[TestMethod]
		public async Task TestGetSetProfile()
		{
			var p = await remoteGateway.GetProfile();
			Assert.IsTrue((p.ID, p.DisplayName, p.Tagline, p.Bio, p.Avatar) == ("id", "displayname", "tagline", "bio", "avatar"));
			var n = new SetProfileRequest("newdisplayname", "newtagline", "newbio", "newavatar");
			await remoteGateway.SetProfile(n);
			p = await remoteGateway.GetProfile();
			Assert.IsTrue(new SetProfileRequest(p.DisplayName, p.Tagline, p.Bio, p.Avatar) == n);
		}

		[TestCleanup]
		public async Task Cleanup()
		{
			localGateway.Dispose();
			await running;
			remoteGateway.Dispose();
		}
	}

	public class MockAPI : Hosta.API.API
	{
		public GetProfileResponse storedProfile = new GetProfileResponse("id", "displayname", "tagline", "bio", "avatar", DateTime.Now);

		public override Task<GetProfileResponse> GetProfile(PublicIdentity _)
		{
			return Task.FromResult(storedProfile);
		}

		public override Task SetProfile(SetProfileRequest profile, PublicIdentity _)
		{
			storedProfile = new GetProfileResponse(storedProfile.ID, profile.DisplayName, profile.Tagline, profile.Bio, profile.Avatar, DateTime.Now);
			return Task.CompletedTask;
		}
	}
}