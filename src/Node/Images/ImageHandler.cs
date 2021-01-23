using Hosta.API;
using Hosta.API.Image;
using Hosta.Crypto;
using Node.Users;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Node.Images
{
	internal class ImageHandler
	{
		readonly private SQLiteAsyncConnection conn;

		readonly private UserHandler users;

		private ImageHandler(SQLiteAsyncConnection conn, UserHandler users)
		{
			this.conn = conn;
			this.users = users;
		}

		public static async Task<ImageHandler> Create(SQLiteAsyncConnection conn, UserHandler self)
		{
			await conn.CreateTableAsync<Image>();
			return new ImageHandler(conn, self);
		}

		public async Task<string> Add(AddImageRequest request, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			var resource = Image.FromAddRequest(request);
			await conn.InsertOrReplaceAsync(resource);
			return resource.Hash;
		}

		public async Task<GetImageResponse> Get(string hash, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.NotBlocked);

			var p = await conn.GetAsync<Image>(hash);
			return p.ToResponse();
		}

		public async Task<List<ImageInfo>> GetList(PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

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

		public async Task Remove(string hash, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			var num = await conn.DeleteAsync<Image>(hash);
			if (num == 0) throw new APIException("Image could not be found!");
		}
	}
}