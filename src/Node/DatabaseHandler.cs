using Hosta.API;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.RPC;
using Node.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
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

		private DatabaseHandler(string path, string admin)
		{
			this.conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
			this.self = admin;
		}

		public static async Task<DatabaseHandler> Create(string path, string admin)
		{
			var h = new DatabaseHandler(path, admin);

			await h.InitProfile();
			await h.conn.CreateTableAsync<Image>();
			await h.conn.CreateTableAsync<Post>();

			return new DatabaseHandler(path, admin);
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

		//// Implementations

		public override async Task<string> AddImage(AddImageRequest request, PublicIdentity client)
		{
			ThrowIfDisposed();
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			var resource = Image.FromAddRequest(request);

			try
			{
				await conn.InsertOrReplaceAsync(resource);
				return resource.Hash;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new RPException("Database error.");
			}
		}

		public override async Task<GetImageResponse> GetImage(string hash, PublicIdentity client)
		{
			ThrowIfDisposed();
			try
			{
				var p = await conn.GetAsync<Image>(hash);
				return p.ToResponse();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new RPException("Database error.");
			}
			throw new Exception();
		}

		public override async Task<List<ImageInfo>> GetImageList(PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			try
			{
				var images = await conn.Table<Image>().ToListAsync();
				List<ImageInfo> info = new();
				foreach (var image in images)
				{
					info.Add(new ImageInfo
					{
						Hash = image.Hash,
						LastUpdated = image.LastUpdated
					});
				}
				return info;
			}
			catch
			{
				throw new RPException("Database error.");
			}
		}

		public override async Task RemoveImage(string hash, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}
			try
			{
				var num = await conn.DeleteAsync<Image>(hash);
				if (num == 0) throw new RPException("Image could not be found!");
			}
			catch (Exception e) when (e is not RPException)
			{
				throw new RPException("Database error.");
			}
		}

		public override async Task<string> AddPost(AddPostRequest request, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			var post = Post.FromAddRequest(request);
			try
			{
				var num = await conn.InsertAsync(post);
				if (num == 0) throw new RPException("Image could not be found!");
				return post.ID;
			}
			catch (Exception e) when (e is not RPException)
			{
				throw new RPException("Database error.");
			}
		}

		public override async Task<GetPostResponse> GetPost(string id, PublicIdentity client)
		{
			ThrowIfDisposed();
			try
			{
				var post = await conn.GetAsync<Post>(id);
				return post.ToResponse();
			}
			catch
			{
				throw new RPException("Database error.");
			}
		}

		public override async Task<List<PostInfo>> GetPostList(DateTime start, PublicIdentity _)
		{
			ThrowIfDisposed();
			try
			{
				var posts = await conn.Table<Post>().Where(x => start < x.TimePosted).ToListAsync();
				return posts.Select(x => new PostInfo { ID = x.ID, TimePosted = x.TimePosted }).ToList();
			}
			catch
			{
				throw new RPException("Database error.");
			}
		}

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