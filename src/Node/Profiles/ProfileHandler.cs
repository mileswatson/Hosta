using Hosta.API;
using Hosta.API.Profile;
using Hosta.Crypto;
using Node.Users;
using SQLite;
using System;
using System.Threading.Tasks;

namespace Node.Profiles
{
	internal class ProfileHandler
	{
		readonly private SQLiteAsyncConnection conn;

		readonly private UserHandler users;

		private ProfileHandler(SQLiteAsyncConnection conn, UserHandler users)
		{
			this.conn = conn;
			this.users = users;
		}

		public static async Task<ProfileHandler> Create(SQLiteAsyncConnection conn, UserHandler users)
		{
			await conn.CreateTableAsync<Profile>();
			try
			{
				var profile = await conn.GetAsync<Profile>(users.Self);
				Console.WriteLine($"Loaded {profile}.");
			}
			catch
			{
				var profile = new Profile
				{
					ID = users.Self,
					Name = "oldname",
					Tagline = "oldtagline",
					Bio = "oldbio"
				};
				await conn.InsertAsync(profile);
				Console.WriteLine($"Created new profile {profile}.");
			}
			return new ProfileHandler(conn, users);
		}

		public async Task<GetProfileResponse> Get(PublicIdentity _)
		{
			var p = await conn.GetAsync<Profile>(users.Self);
			return p.ToResponse();
		}

		public async Task Set(SetProfileRequest r, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			if (r.Name.Length > 18)
				throw new APIException($"Name used {r.Name.Length}/18 characters.");
			if (r.Tagline.Length > 30)
				throw new APIException($"Tagline used {r.Tagline.Length}/30 characters.");
			if (r.Bio.Length > 400)
				throw new APIException($"Bio used {r.Bio.Length}/400 characters.");

			await conn.InsertOrReplaceAsync(Profile.FromSetRequest(r, users.Self));
		}
	}
}