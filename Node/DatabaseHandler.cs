using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using Node.Data;
using System;
using System.Threading.Tasks;
using SQLite;
using System.Diagnostics;
using Hosta.Tools;
using System.Collections.Generic;

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

		//// Functionality

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
				throw new Exception("Database error!");
			}
		}

		/// <summary>
		/// Set the profile name.
		/// </summary>
		public override async Task<string> SetProfile(SetProfileRequest r, PublicIdentity client)
		{
			ThrowIfDisposed();
			// TODO: Change this to what it should actually be (!=)
			if (client.ID == self)
			{
				throw new Exception("Access denied!");
			}

			try
			{
				var s = await conn.InsertOrReplaceAsync(new Profile(self, r));
				return "";
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new Exception("Database error!");
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