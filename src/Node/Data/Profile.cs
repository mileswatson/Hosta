using Hosta.API.Data;
using SQLite;

namespace Node.Data
{
	/// <summary>
	/// Represents a user profile.
	/// </summary>
	internal record Profile
	{
		[PrimaryKey]
		public string ID { get; init; }

		public string Name { get; init; }

		public string Tagline { get; init; }

		public string Bio { get; init; }

		public string AvatarResource { get; init; }

		public Profile()
		{
			ID = "";
			Name = "";
			Tagline = "";
			Bio = "";
			AvatarResource = "";
		}

		public static Profile FromSetRequest(SetProfileRequest r, string id) => new Profile
		{
			ID = id,
			Name = r.Name,
			Tagline = r.Tagline,
			Bio = r.Bio,
			AvatarResource = r.AvatarHash
		};

		public GetProfileResponse ToResponse() => new GetProfileResponse
		{
			Name = Name,
			Tagline = Tagline,
			Bio = Bio,
			AvatarHash = AvatarResource
		};
	}
}