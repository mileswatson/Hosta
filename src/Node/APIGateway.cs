using Hosta.API;
using Hosta.API.Address;
using Hosta.API.Friend;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.Tools;
using Node.Addresses;
using Node.Images;
using Node.Posts;
using Node.Profiles;
using Node.Users;
using SQLite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Node
{
	/// <summary>
	/// Forwards API requests to respective handlers.
	/// </summary>
	internal class APIGateway : API
	{
		private readonly UserHandler users;

		private readonly ImageHandler images;

		private readonly PostHandler posts;

		private readonly ProfileHandler profiles;

		private readonly AddressHandler addresses;

		private APIGateway(UserHandler users, ImageHandler images, PostHandler posts, ProfileHandler profiles, AddressHandler addresses)

		{
			this.users = users;
			this.images = images;
			this.posts = posts;
			this.profiles = profiles;
			this.addresses = addresses;
		}

		public static async Task<APIGateway> Create(string path, PrivateIdentity self, int port)
		{
			var conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

			var users = await UserHandler.Create(conn, self.ID);

			var images = await ImageHandler.Create(conn, users);

			var posts = await PostHandler.Create(conn, users);

			var profiles = await ProfileHandler.Create(conn, users);

			var addresses = await AddressHandler.Create(conn, users, port, self);

			return new APIGateway(users, images, posts, profiles, addresses);
		}

		public static async Task Call(Func<Task> action)
		{
			try
			{
				await action();
			}
			catch (Exception e) when (e is not APIException)
			{
				throw new APIException("Database error.");
			}
		}

		public static async Task<T> Call<T>(Func<Task<T>> function)
		{
			try
			{
				return await function();
			}
			catch (Exception e) when (e is not APIException)
			{
				throw new APIException("Database error.");
			}
		}

		//// Implementations

		public override Task AddAddress(Tuple<string, AddressInfo> address, PublicIdentity? client) =>
			Call(() => addresses.AddAddress(address.Item1, IPAddress.Parse(address.Item2.IP), address.Item2.Port, client.GuardNull()));

		public override Task<Dictionary<string, AddressInfo>> GetAddresses(List<string> users, PublicIdentity? client) =>
			Call(() => addresses.GetAddresses(users, client.GuardNull()));

		public override Task InformAddress(int port, IPAddress? address, PublicIdentity? client) =>
			Call(() => addresses.InformAddress(port, address.GuardNull(), client.GuardNull()));

		public override Task<List<FriendInfo>> GetFriendList(PublicIdentity? client) =>
			Call(() => users.GetFriendList(client.GuardNull()));

		public override Task SetFriend(FriendInfo info, PublicIdentity? client) =>
			Call(() => users.SetFriend(info, client.GuardNull()));

		public override Task RemoveFriend(string user, PublicIdentity? client) =>
			Call(() => users.RemoveFriend(user, client.GuardNull()));

		public override Task<string> AddImage(AddImageRequest request, PublicIdentity? client) =>
			Call(() => images.Add(request, client.GuardNull()));

		public override Task<GetImageResponse> GetImage(string hash, PublicIdentity? client) =>
			Call(() => images.Get(hash, client.GuardNull()));

		public override Task<List<ImageInfo>> GetImageList(PublicIdentity? client) =>
			Call(() => images.GetList(client.GuardNull()));

		public override Task RemoveImage(string hash, PublicIdentity? client) =>
			Call(() => images.Remove(hash, client.GuardNull()));

		public override Task<string> AddPost(AddPostRequest request, PublicIdentity? client) =>
			Call(() => posts.Add(request, client.GuardNull()));

		public override Task<GetPostResponse> GetPost(string id, PublicIdentity? client) =>
			Call(() => posts.Get(id, client.GuardNull()));

		public override Task<List<PostInfo>> GetPostList(DateTimeOffset start, PublicIdentity? client) =>
			Call(() => posts.GetList(start, client.GuardNull()));

		public override Task RemovePost(string id, PublicIdentity? client) =>
			Call(() => posts.RemovePost(id, client.GuardNull()));

		public override Task<GetProfileResponse> GetProfile(PublicIdentity? client) =>
			Call(() => profiles.Get(client.GuardNull()));

		public override Task SetProfile(SetProfileRequest request, PublicIdentity? client) =>
			Call(() => profiles.Set(request, client.GuardNull()));
	}
}