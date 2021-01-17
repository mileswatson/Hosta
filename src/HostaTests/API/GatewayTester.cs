using Hosta.API;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
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
		public async Task GetSetProfile()
		{
			var p = await remoteGateway.GetProfile();
			Assert.IsTrue(p == new GetProfileResponse());
			var n = new SetProfileRequest
			{
				Name = "name",
				Tagline = "tagline",
				Bio = "bio",
				AvatarHash = "avatarresource"
			};
			await remoteGateway.SetProfile(n);
			p = await remoteGateway.GetProfile();
			Assert.IsTrue((p.Name, p.Tagline, p.Bio) == (n.Name, n.Tagline, n.Bio));
		}

		[TestMethod]
		public async Task AddGetPosts()
		{
			var img1 = new AddPostRequest { Content = "1" };
			var img2 = new AddPostRequest { Content = "2" };
			var id1 = await remoteGateway.AddPost(img1);
			var list = await remoteGateway.GetPostList(DateTime.MinValue);
			Assert.AreEqual(list.Count, 1);
			var id2 = await remoteGateway.AddPost(img2);
			list = await remoteGateway.GetPostList(DateTime.MinValue);
			Assert.AreEqual(list.Count, 2);
			Assert.AreEqual((await remoteGateway.GetPost(id1)).Content, "1");
			Assert.AreEqual((await remoteGateway.GetPost(id2)).Content, "2");
		}

		[TestMethod]
		public async Task AddGetImage()
		{
			var data = new byte[] { 0, 1, 3, 255, 6, 0 };
			var hash = Transcoder.HexFromBytes(SHA256.HashData(data));
			await Assert.ThrowsExceptionAsync<RPException>(() => remoteGateway.GetImage(hash));
			hash = await remoteGateway.AddImage(new AddImageRequest { Data = data });
			var response = await remoteGateway.GetImage(hash);
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

		private readonly Dictionary<string, byte[]> images = new();

		private readonly Dictionary<string, GetPostResponse> posts = new();

		public override Task<string> AddImage(AddImageRequest request, PublicIdentity _)
		{
			var hash = Transcoder.HexFromBytes(SHA256.HashData(request.Data));
			images[hash] = request.Data;
			return Task.FromResult(hash);
		}

		public override Task<GetImageResponse> GetImage(string hash, PublicIdentity _)
		{
			return Task.FromResult(new GetImageResponse
			{
				Data = images[hash],
				LastUpdated = DateTime.Now
			});
		}

		public override Task<List<ImageInfo>> GetImageList(PublicIdentity _)
		{
			throw new NotImplementedException();
		}

		public override Task RemoveImage(string hash, PublicIdentity _)
		{
			images.Remove(hash);
			return Task.CompletedTask;
		}

		public override Task<string> AddPost(AddPostRequest request, PublicIdentity _)
		{
			var id = Transcoder.HexFromBytes(SecureRandomGenerator.GetBytes(32));
			posts[id] = new GetPostResponse
			{
				Content = request.Content,
				ImageHash = request.ImageHash,
				TimePosted = DateTime.Now
			};
			return Task.FromResult(id);
		}

		public override Task<GetPostResponse> GetPost(string id, PublicIdentity _)
		{
			var request = posts[id];
			return Task.FromResult(new GetPostResponse
			{
				Content = request.Content,
				ImageHash = request.ImageHash,
				TimePosted = DateTime.Now
			});
		}

		public override Task<List<PostInfo>> GetPostList(DateTime start, PublicIdentity _)
		{
			var list = new List<PostInfo>();
			foreach (var kvp in posts)
			{
				var id = kvp.Key;
				var post = kvp.Value;
				if (post.TimePosted > start)
				{
					list.Add(new PostInfo { ID = id, TimePosted = post.TimePosted });
				}
			}
			return Task.FromResult(list);
		}

		public override Task RemovePost(string id, PublicIdentity _)
		{
			posts.Remove(id);
			return Task.CompletedTask;
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
				AvatarHash = profile.AvatarHash
			};
			return Task.CompletedTask;
		}
	}
}