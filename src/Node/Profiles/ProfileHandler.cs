using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.RPC;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Node.Profiles
{
	internal class ProfileHandler
	{
		private readonly SQLiteAsyncConnection conn;

		private readonly string self;

		private ProfileHandler(SQLiteAsyncConnection conn, string self)
		{
			this.conn = conn;
			this.self = self;
		}

		public static async Task<ProfileHandler> Create(SQLiteAsyncConnection conn, string self)
		{
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
			return new ProfileHandler(conn, self);
		}

		public async Task<GetProfileResponse> GetProfile(PublicIdentity _)
		{
			var p = await conn.GetAsync<Profile>(self);
			return p.ToResponse();
		}

		public async Task SetProfile(SetProfileRequest r, PublicIdentity client)
		{
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

			await conn.InsertOrReplaceAsync(Profile.FromSetRequest(r, self));
		}
	}
}