using Hosta.API.Profile;

namespace ClientWPF.Models.Data
{
	public record Profile
	{
		public string Name { get; init; }

		public string ID { get; init; }

		public string Tagline { get; init; }

		public string Bio { get; init; }

		public string AvatarHash { get; init; }

		public Profile()
		{
			Name = "";
			ID = "";
			Tagline = "";
			Bio = "";
			AvatarHash = "";
		}

		public static Profile FromResponse(GetProfileResponse response, string id) => new Profile
		{
			Name = response.Name,
			ID = id,
			Tagline = response.Tagline,
			Bio = response.Bio,
			AvatarHash = response.AvatarHash
		};
	}
}