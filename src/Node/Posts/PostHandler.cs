using Hosta.API;
using Hosta.API.Post;
using Hosta.Crypto;
using Node.Users;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Node.Posts
{
	internal class PostHandler
	{
		readonly private SQLiteAsyncConnection conn;

		readonly private UserHandler users;

		private PostHandler(SQLiteAsyncConnection conn, UserHandler users)
		{
			this.conn = conn;
			this.users = users;
		}

		public static async Task<PostHandler> Create(SQLiteAsyncConnection conn, UserHandler users)
		{
			await conn.CreateTableAsync<Post>();
			return new PostHandler(conn, users);
		}

		public async Task<string> Add(AddPostRequest request, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			var post = Post.FromAddRequest(request);
			var num = await conn.InsertAsync(post);
			if (num == 0) throw new APIException("Image could not be found!");
			return post.ID;
		}

		public async Task<GetPostResponse> Get(string id, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.NotBlocked);

			var post = await conn.GetAsync<Post>(id);
			return post.ToResponse();
		}

		public async Task<List<PostInfo>> GetList(DateTimeOffset start, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Friend);

			var posts = await conn.Table<Post>().Where(x => start < x.TimePosted).ToListAsync();
			return posts.Select(x => new PostInfo { ID = x.ID, TimePosted = x.TimePosted }).ToList();
		}

		public async Task RemovePost(string id, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			var num = await conn.DeleteAsync<Post>(id);
			if (num == 0) throw new APIException("Image could not be found!");
		}
	}
}