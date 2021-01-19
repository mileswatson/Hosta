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
	/// Handles API requests using an SQLite database.
	/// </summary>
	internal class DatabaseHandler : API, IDisposable
	{
		private readonly string self;

		private readonly SQLiteAsyncConnection conn;

		private readonly ImageHandler images;

		private readonly PostHandler posts;

		private DatabaseHandler(string self, SQLiteAsyncConnection conn, ImageHandler images, PostHandler posts)
		{
			this.self = self;
			this.conn = conn;
			this.images = images;
			this.posts = posts;
		}

		public static async Task<DatabaseHandler> Create(string path, string self)
		{
			var conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

			var images = await ImageHandler.Create(conn, self);

			var posts = await PostHandler.Create(conn, self);

			var h = new DatabaseHandler(self, conn, images, posts);

			await h.InitProfile();

			return h;
		}

		private async Task InitProfile()
		{
			ThrowIfDisposed();
			await conn.CreateTableAsync<Profile>();
			try
			{
				var profile = await conn.GetAsync<Profile>(self);
				Console.WriteLine($"Loaded {profile}.");
			}
			catch
			{
				var profile = new Profile
				{
					ID = self,
					Name = "oldname",
					Tagline = "oldtagline",
					Bio = "oldbio"
				};
				await conn.InsertAsync(profile);
				Console.WriteLine($"Created new profile {profile}.");
			}
		}

		public async Task SafeCall(Func<Task> action)
		{
			ThrowIfDisposed();
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
			ThrowIfDisposed();
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

		public override async Task<GetProfileResponse> GetProfile(PublicIdentity _)
		{
			ThrowIfDisposed();
			try
			{
				var p = await conn.GetAsync<Profile>(self);
				return p.ToResponse();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new RPException("Database error.");
			}
		}

		public override async Task<string> SetProfile(SetProfileRequest r, PublicIdentity client)
		{
			ThrowIfDisposed();
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			if (r.Name.Length > 18)
				throw new RPException($"Name used {r.Name.Length}/18 characters.");
			if (r.Tagline.Length > 30)
				throw new RPException($"Tagline used {r.Tagline.Length}/30 characters.");
			if (r.Bio.Length > 200)
				throw new RPException($"Bio used {r.Bio.Length}/200 characters.");

			try
			{
				await conn.InsertOrReplaceAsync(Profile.FromSetRequest(r, self));
				return "";
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new RPException("Database error.");
			}
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("Attempted post-disposal use!");
		}

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
				// Dispose of database
			}

			disposed = true;
		}
	}
}