﻿using Hosta.API;
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
				var profile = new Profile(self, "olddisplayname", "oldtagline", "oldbio", "oldavatar", DateTime.UtcNow);
				await conn.InsertAsync(profile);
				Console.WriteLine($"Created new profile {profile}.");
			}
		}

		//// Implementations

		/// <summary>
		/// Get the profile name.
		/// </summary>
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
				throw new RPException("Database error!");
			}
		}

		/// <summary>
		/// Set the profile name.
		/// </summary>
		public override async Task<string> SetProfile(SetProfileRequest r, PublicIdentity client)
		{
			ThrowIfDisposed();
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			if (r.DisplayName.Length > 18)
				throw new RPException($"Name used {r.DisplayName.Length}/18 characters.");
			if (r.Tagline.Length > 30)
				throw new RPException($"Tagline used {r.Tagline.Length}/30 characters.");
			if (r.Bio.Length > 200)
				throw new RPException($"Bio used {r.Bio.Length}/200 characters.");

			try
			{
				var s = await conn.InsertOrReplaceAsync(new Profile(self, r));
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