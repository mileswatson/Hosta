using Hosta.API;
using Hosta.Crypto;
using Hosta.Tools;
using Node.Data;
using System;
using System.Threading.Tasks;
using SQLite;
using System.Diagnostics;

namespace Node
{
	/// <summary>
	/// Handles API requests using an SQLite database.
	/// </summary>
	internal class DatabaseHandler : API, IDisposable
	{
		private readonly SQLiteAsyncConnection conn;
		private readonly SQLiteAsyncConnection writeConn;

		private DatabaseHandler(string path)
		{
			this.conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex);
			this.writeConn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
		}

		public static async Task<DatabaseHandler> Create(string path)
		{
			var initConn = new SQLiteAsyncConnection(
				path,
				SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex
			);

			await InitProfile(initConn);

			return new DatabaseHandler(path);
		}

		private static async Task InitProfile(SQLiteAsyncConnection conn)
		{
			await conn.CreateTableAsync<ProfileField>();
			try
			{
				var field = await conn.GetAsync<ProfileField>("Name");
				Console.WriteLine($"Loaded {field.Key}:{field.Value}");
			}
			catch
			{
				await conn.InsertAsync(new ProfileField("Name", "Firstname Lastname"));
				Console.WriteLine("Created profile field: Name");
			}
		}

		//// Functionality

		/// <summary>
		/// Demo functionality.
		/// </summary>
		public override async Task<string> GetName(PublicIdentity _)
		{
			ThrowIfDisposed();
			try
			{
				var r = await conn.GetAsync<ProfileField>("Name");
				return r.Value;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw new Exception("Invalid profile field!");
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