using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using Hosta.RPC;
using Hosta.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
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
			Assert.IsTrue(p == new GetProfileResponse());
			var n = new SetProfileRequest
			{
				Name = "name",
				Tagline = "tagline",
				Bio = "bio",
				AvatarResource = "avatarresource"
			};
			await remoteGateway.SetProfile(n);
			p = await remoteGateway.GetProfile();
			Assert.IsTrue((p.Name, p.Tagline, p.Bio) == (n.Name, n.Tagline, n.Bio));
		}

		[TestMethod]
		public async Task TestAddGetResource()
		{
			var data = new byte[] { 0, 1, 3, 255, 6, 0 };
			var hash = Transcoder.HexFromBytes(SHA256.HashData(data));
			await Assert.ThrowsExceptionAsync<RPException>(() => remoteGateway.GetResource(hash));
			hash = await remoteGateway.AddResource(new AddResourceRequest { Data = data });
			var response = await remoteGateway.GetResource(hash);
			CollectionAssert.AreEqual(data, response.Data);
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
		private GetProfileResponse storedProfile = new();

		private readonly Dictionary<string, byte[]> resources = new();

		public override Task<string> AddResource(AddResourceRequest request, PublicIdentity _)
		{
			var hash = Transcoder.HexFromBytes(SHA256.HashData(request.Data));
			resources[hash] = request.Data;
			return Task.FromResult(hash);
		}

		public override Task<GetProfileResponse> GetProfile(PublicIdentity _)
		{
			return Task.FromResult(storedProfile);
		}

		public override Task SetProfile(SetProfileRequest profile, PublicIdentity _)
		{
			storedProfile = new GetProfileResponse
			{
				Name = profile.Name,
				Tagline = profile.Tagline,
				Bio = profile.Bio,
				AvatarResource = profile.AvatarResource
			};
			return Task.CompletedTask;
		}

		public override Task<GetResourceResponse> GetResource(string hash, PublicIdentity _)
		{
			return Task.FromResult(new GetResourceResponse
			{
				Data = resources[hash],
				LastUpdated = DateTime.Now
			});
		}
	}
}