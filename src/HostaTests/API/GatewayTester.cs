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
			Assert.IsTrue((p.ID, p.DisplayName, p.Tagline, p.Bio) == ("id", "displayname", "tagline", "bio"));
			CollectionAssert.AreEqual(p.Avatar, new byte[] { 0, 255, 0, 5, 0 });
			var n = new SetProfileRequest("newdisplayname", "newtagline", "newbio", new byte[] { 5, 0, 255, 0, 3 });
			await remoteGateway.SetProfile(n);
			p = await remoteGateway.GetProfile();
			Assert.IsTrue((p.DisplayName, p.Tagline, p.Bio) == (n.DisplayName, n.Tagline, n.Bio));
			CollectionAssert.AreEqual(p.Avatar, n.Avatar);
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
		public GetProfileResponse storedProfile = new GetProfileResponse("id", "displayname", "tagline", "bio", new byte[] { 0, 255, 0, 5, 0 }, DateTime.Now);

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