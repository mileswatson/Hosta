using Newtonsoft.Json;
using System;

namespace Hosta.API.Data
{
	/// <summary>
	/// Represents the request for a SetProfile operation.
	/// </summary>
	public record SetProfileRequest
	{
		public SetProfileRequest() => (DisplayName, Tagline, Bio, Avatar) = ("", "", "", "");

		public SetProfileRequest(string displayName, string tagline, string bio, string avatar)
			=> (DisplayName, Tagline, Bio, Avatar) = (displayName, tagline, bio, avatar);

		/// <summary>
		/// Exports SetProfileRequest as a JSON string.
		/// </summary>
		public static string Export(SetProfileRequest request)
		{
			return JsonConvert.SerializeObject(request);
		}

		/// <summary>
		/// Creates a new SetProfileRequest from a JSON string.
		/// </summary>
		public static SetProfileRequest Import(string json)
		{
			var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };
			return JsonConvert.DeserializeObject<SetProfileRequest>(json, settings) ?? throw new Exception("Could not import SetProfileRequest!");
		}

		[JsonProperty(Required = Required.Always)]
		public string DisplayName { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Tagline { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Bio { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Avatar { get; init; }
	}
}