﻿using Newtonsoft.Json;

namespace Hosta.API.Profile
{
	/// <summary>
	/// Represents a response to a GetProfile call.
	/// </summary>
	public record GetProfileResponse
	{
		[JsonProperty(Required = Required.Always)]
		public string Name { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Tagline { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Bio { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string AvatarHash { get; init; }

		public GetProfileResponse()
		{
			Name = "";
			Tagline = "";
			Bio = "";
			AvatarHash = "";
		}
	}
}