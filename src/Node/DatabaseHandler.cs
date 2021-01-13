using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using Hosta.RPC;
using Node.Data;
using SQLite;
using System;
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
			await h.conn.CreateTableAsync<Blob>();

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

		public override async Task<string> AddBlob(AddBlobRequest request, PublicIdentity client)
		{
			ThrowIfDisposed();
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			var resource = Blob.FromAddRequest(request);

			try
			{
				await conn.InsertOrReplaceAsync(resource);
				Console.WriteLine(resource.Hash);
				return resource.Hash;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
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

		public override async Task<GetBlobResponse> GetBlob(string hash, PublicIdentity client)
		{
			ThrowIfDisposed();
			try
			{
				var p = await conn.GetAsync<Blob>(hash);
				return p.ToResponse();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new RPException("Database error.");
			}
			throw new Exception();
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