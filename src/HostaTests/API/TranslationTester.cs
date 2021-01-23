using Hosta.API;
using Hosta.API.Friend;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HostaTests.API
{
	[TestClass]
	public class TranslationTester
	{
		private readonly PrivateIdentity server = PrivateIdentity.Create();
		private readonly PrivateIdentity client = PrivateIdentity.Create();

		private readonly Hosta.API.API api = new MockAPI();

		private readonly APITranslationServer localGateway;

		private readonly Task running;

		private readonly APITranslatorClient remoteGateway;

		public TranslationTester()
		{
			localGateway = new APITranslationServer(server, new IPEndPoint(IPAddress.Loopback, 12000), api);
			running = localGateway.Run();
			var args = new APITranslatorClient.ConnectionArgs { Address = IPAddress.Loopback, Port = 12000, Self = client, ServerID = server.ID };
			remoteGateway = APITranslatorClient.CreateAndConnect(args).Result;
		}

		[TestMethod]
		public async Task SetGetFriends()
		{
			var friend = new FriendInfo { ID = "id", Name = "name", IsFavorite = false };
			await remoteGateway.SetFriend(friend);
			var list = await remoteGateway.GetFriendList();
			Assert.AreEqual(list[0], friend);
			friend = new FriendInfo { ID = "id", Name = "othername", IsFavorite = true };
			await remoteGateway.SetFriend(friend);
			list = await remoteGateway.GetFriendList();
			Assert.AreEqual(list[0], friend);
			await remoteGateway.RemoveFriend("id");
			list = await remoteGateway.GetFriendList();
			Assert.IsTrue(list.Count == 0);
		}

		[TestMethod]
		public async Task AddGetImage()
		{
			var data = new byte[] { 0, 1, 3, 255, 6, 0 };
			var hash = Transcoder.HexFromBytes(SHA256.HashData(data));
			await Assert.ThrowsExceptionAsync<APIException>(() => remoteGateway.GetImage(hash));
			hash = await remoteGateway.AddImage(new AddImageRequest { Data = data });
			var response = await remoteGateway.GetImage(hash);
			CollectionAssert.AreEqual(data, response.Data);
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

		private readonly Dictionary<string, FriendInfo> friends = new();

		public override Task InformAddress(IPEndPoint? _1 = null, PublicIdentity? _2 = null)
		{
			throw new NotImplementedException();
		}

		public override Task<List<FriendInfo>> GetFriendList(PublicIdentity _)
		{
			return Task.FromResult(friends.Select(kvp => kvp.Value).ToList());
		}

		public override Task RemoveFriend(string user, PublicIdentity _)
		{
			friends.Remove(user);
			return Task.CompletedTask;
		}

		public override Task SetFriend(FriendInfo info, PublicIdentity _)
		{
			friends[info.ID] = info;
			return Task.CompletedTask;
		}

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
				LastUpdated = DateTimeOffset.Now
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
				TimePosted = DateTimeOffset.Now
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
				TimePosted = DateTimeOffset.Now
			});
		}

		public override Task<List<PostInfo>> GetPostList(DateTimeOffset start, PublicIdentity _)
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