using Hosta.API;
using Hosta.API.Friend;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Node.Users;
using Node.Images;
using Node.Posts;
using Node.Profiles;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Node.Addresses;
using Hosta.API.Address;

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

		public static async Task<APIGateway> Create(string path, string self)
		{
			var conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

			var users = await UserHandler.Create(conn, self);

			var images = await ImageHandler.Create(conn, users);

			var posts = await PostHandler.Create(conn, users);

			var profiles = await ProfileHandler.Create(conn, users);

			var addresses = await AddressHandler.Create(conn, users);

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

		public override Task<Dictionary<string, AddressInfo>> GetAddresses(List<string> users, PublicIdentity client) =>
			Call(() => addresses.GetAddresses(users, client));

		public override Task InformAddress(IPEndPoint address, PublicIdentity client) =>
			Call(() => addresses.InformAddress(address, client));

		public override Task<List<FriendInfo>> GetFriendList(PublicIdentity client) =>
			Call(() => users.GetFriendList(client));

		public override Task SetFriend(FriendInfo info, PublicIdentity client) =>
			Call(() => users.SetFriend(info, client));

		public override Task RemoveFriend(string user, PublicIdentity client) =>
			Call(() => users.RemoveFriend(user, client));

		public override Task<string> AddImage(AddImageRequest request, PublicIdentity client) =>
			Call(() => images.Add(request, client));

		public override Task<GetImageResponse> GetImage(string hash, PublicIdentity client) =>
			Call(() => images.Get(hash, client));

		public override Task<List<ImageInfo>> GetImageList(PublicIdentity client) =>
			Call(() => images.GetList(client));

		public override Task RemoveImage(string hash, PublicIdentity client) =>
			Call(() => images.Remove(hash, client));

		public override Task<string> AddPost(AddPostRequest request, PublicIdentity client) =>
			Call(() => posts.Add(request, client));

		public override Task<GetPostResponse> GetPost(string id, PublicIdentity client) =>
			Call(() => posts.Get(id, client));

		public override Task<List<PostInfo>> GetPostList(DateTimeOffset start, PublicIdentity client) =>
			Call(() => posts.GetList(start, client));

		public override Task RemovePost(string id, PublicIdentity client) =>
			Call(() => posts.RemovePost(id, client));

		public override Task<GetProfileResponse> GetProfile(PublicIdentity client) =>
			Call(() => profiles.Get(client));

		public override Task SetProfile(SetProfileRequest request, PublicIdentity client) =>
			Call(() => profiles.Set(request, client));
	}
}