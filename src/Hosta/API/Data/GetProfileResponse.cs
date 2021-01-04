using Newtonsoft.Json;
using System;

namespace Hosta.API.Data
{
	/// <summary>
	/// Represents a response to a GetProfile call.
	/// </summary>
	public record GetProfileResponse
	{
		public GetProfileResponse() => (ID, DisplayName, Tagline, Bio, Avatar, LastUpdated) = ("", "", "", "", "", DateTime.UtcNow);

		public GetProfileResponse(string id, string displayName, string tagline, string bio, string avatar, DateTime lastUpdated)
			=> (ID, DisplayName, Tagline, Bio, Avatar, LastUpdated) = (id, displayName, tagline, bio, avatar, lastUpdated);

		/// <summary>
		/// Exports the GetProfileResponse as a JSON string.
		/// </summary>
		public static string Export(GetProfileResponse response)
		{
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// Creates a GetProfileResponse from a JSON string.
		/// </summary>
		public static GetProfileResponse Import(string json)
		{
			var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };
			return JsonConvert.DeserializeObject<GetProfileResponse>(json, settings) ?? throw new Exception("Could not import GetProfileResponse!");
		}

		[JsonProperty(Required = Required.Always)]
		public string ID { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string DisplayName { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Tagline { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Bio { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Avatar { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTime LastUpdated { get; init; }
	}
}