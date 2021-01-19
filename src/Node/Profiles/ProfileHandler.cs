using Hosta.API;
using Hosta.API.Profile;
using Hosta.Crypto;
using SQLite;
using System;
using System.Threading.Tasks;

namespace Node.Profiles
{
	internal class ProfileHandler : DatabaseHandler
	{
		private ProfileHandler(SQLiteAsyncConnection conn, string self) : base(conn, self)
		{
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

		public async Task<GetProfileResponse> Get(PublicIdentity _)
		{
			var p = await conn.GetAsync<Profile>(self);
			return p.ToResponse();
		}

		public async Task Set(SetProfileRequest r, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new APIException("Access denied.");
			}

			if (r.Name.Length > 18)
				throw new APIException($"Name used {r.Name.Length}/18 characters.");
			if (r.Tagline.Length > 30)
				throw new APIException($"Tagline used {r.Tagline.Length}/30 characters.");
			if (r.Bio.Length > 200)
				throw new APIException($"Bio used {r.Bio.Length}/200 characters.");

			await conn.InsertOrReplaceAsync(Profile.FromSetRequest(r, self));
		}
	}
}