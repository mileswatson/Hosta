using Hosta.API;
using Hosta.API.Friend;
using Hosta.Crypto;
using Hosta.Tools;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Node.Users
{
	internal class UserHandler
	{
		private readonly SQLiteAsyncConnection conn;

		public string Self { get; init; }

		private UserHandler(SQLiteAsyncConnection conn, string self)
		{
			this.conn = conn;
			this.Self = self;
		}

		public static async Task<UserHandler> Create(SQLiteAsyncConnection conn, string self)
		{
			await conn.CreateTableAsync<User>();
			return new UserHandler(conn, self);
		}

		public async Task Authenticate(PublicIdentity client, User.Auth minimumAuth)
		{
			if (await GetAuthLevel(client.ID) >= minimumAuth) return;
			throw new AuthenticationException();
		}

		public async Task<User.Auth> GetAuthLevel(string clientID)
		{
			if (clientID == Self)
			{
				return User.Auth.Self;
			}
			try
			{
				var user = await conn.GetAsync<User>(clientID);
				return user.AuthLevel;
			}
			catch (KeyNotFoundException)
			{
				return User.Auth.NotBlocked;
			}
		}

		public async Task<List<FriendInfo>> GetFriendList(PublicIdentity client)
		{
			await Authenticate(client, User.Auth.Self);

			var users = await conn.Table<User>()
				.Where(user => user.AuthLevel == User.Auth.Friend || user.AuthLevel == User.Auth.Self)
				.ToListAsync();

			List<FriendInfo> friends = new();
			foreach (var user in users)
			{
				friends.Add(new FriendInfo
				{
					ID = user.UserID,
					Name = user.Name,
					IsFavorite = (user.AuthLevel == User.Auth.Favorite)
				});
			}
			return friends;
		}

		public async Task RemoveFriend(string user, PublicIdentity client)
		{
			await Authenticate(client, User.Auth.Self);

			var userAuthLevel = await GetAuthLevel(user);

			if (userAuthLevel != User.Auth.Friend && userAuthLevel != User.Auth.Favorite)
			{
				throw new APIException("User is not a friend!");
			}

			await conn.DeleteAsync<User>(user);
		}

		public async Task SetFriend(FriendInfo info, PublicIdentity client)
		{
			await Authenticate(client, User.Auth.Self);

			try
			{
				var bytes = Transcoder.BytesFromHex(info.ID);
				if (bytes.Length != 32) throw new Exception();
			}
			catch
			{
				throw new APIException("Invalid ID format.");
			}

			var num = await conn.Table<User>()
			.Where(user => user.Name == info.Name)
			.CountAsync();

			if (num > 0)
			{
				throw new APIException("Name already taken.");
			}

			var friend = new User()
			{
				UserID = info.ID,
				Name = info.Name,
				AuthLevel = info.IsFavorite ? User.Auth.Favorite : User.Auth.Friend
			};

			await conn.InsertOrReplaceAsync(friend);
		}
	}

	public class AuthenticationException : APIException
	{
		public AuthenticationException() : base("Access denied.")
		{
		}
	}
}