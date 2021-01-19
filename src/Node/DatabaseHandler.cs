using Hosta.API;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.RPC;
using Node.Images;
using Node.Posts;
using Node.Profiles;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Node
{
	/// <summary>
	/// Forwards API requests to respective handlers.
	/// </summary>
	internal class DatabaseHandler : API
	{
		private readonly ImageHandler images;

		private readonly PostHandler posts;

		private readonly ProfileHandler profiles;

		private DatabaseHandler(ImageHandler images, PostHandler posts, ProfileHandler profiles)
		{
			this.images = images;
			this.posts = posts;
			this.profiles = profiles;
		}

		public static async Task<DatabaseHandler> Create(string path, string self)
		{
			var conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

			var images = await ImageHandler.Create(conn, self);

			var posts = await PostHandler.Create(conn, self);

			var profiles = await ProfileHandler.Create(conn, self);

			return new DatabaseHandler(images, posts, profiles);
		}

		public async Task SafeCall(Func<Task> action)
		{
			try
			{
				await action();
			}
			catch (Exception e) when (e is not RPException)
			{
				throw new RPException("Database error.");
			}
		}

		public async Task<T> SafeCall<T>(Func<Task<T>> function)
		{
			try
			{
				return await function();
			}
			catch (Exception e) when (e is not RPException)
			{
				throw new RPException("Database error.");
			}
		}

		//// Implementations

		public override Task<string> AddImage(AddImageRequest request, PublicIdentity client) =>
			SafeCall(() => images.Add(request, client));

		public override Task<GetImageResponse> GetImage(string hash, PublicIdentity client) =>
			SafeCall(() => images.Get(hash, client));

		public override Task<List<ImageInfo>> GetImageList(PublicIdentity client) =>
			SafeCall(() => images.GetList(client));

		public override Task RemoveImage(string hash, PublicIdentity client) =>
			SafeCall(() => images.Remove(hash, client));

		public override Task<string> AddPost(AddPostRequest request, PublicIdentity client) =>
			SafeCall(() => posts.Add(request, client));

		public override Task<GetPostResponse> GetPost(string id, PublicIdentity client) =>
			SafeCall(() => posts.Get(id, client));

		public override Task<List<PostInfo>> GetPostList(DateTime start, PublicIdentity client) =>
			SafeCall(() => posts.GetList(start, client));

		public override Task RemovePost(string id, PublicIdentity client) =>
			SafeCall(() => posts.RemovePost(id, client));

		public override Task<GetProfileResponse> GetProfile(PublicIdentity client) =>
			SafeCall(() => profiles.GetProfile(client));

		public override Task SetProfile(SetProfileRequest request, PublicIdentity client) =>
			SafeCall(() => profiles.SetProfile(request, client));
	}
}