using Hosta.API.Friend;
using SQLite;

namespace Node.Users
{
	internal record User
	{
		public enum Auth
		{
			Blocked = -1,
			NotBlocked = 0,
			Friend = 2,
			Favorite = 3,
			Self = 5
		}

		[PrimaryKey]
		public string UserID { get; init; }

		public string Name { get; init; }

		public Auth AuthLevel { get; init; }

		public User()
		{
			UserID = "";
			Name = "";
			AuthLevel = Auth.NotBlocked;
		}
	}
}