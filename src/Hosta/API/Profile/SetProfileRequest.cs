using Newtonsoft.Json;

namespace Hosta.API.Profile
{
	/// <summary>
	/// Represents the request for a SetProfile operation.
	/// </summary>
	public record SetProfileRequest
	{
		[JsonProperty(Required = Required.Always)]
		public string Name { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Tagline { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Bio { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string AvatarHash { get; init; }

		public SetProfileRequest()
		{
			Name = "";
			Tagline = "";
			Bio = "";
			AvatarHash = "";
		}
	}
}