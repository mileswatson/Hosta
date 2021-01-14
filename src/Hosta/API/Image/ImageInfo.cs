using Newtonsoft.Json;
using System;

namespace Hosta.API.Image
{
	public record ImageInfo
	{
		[JsonProperty(Required = Required.Always)]
		public string Hash { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTime LastUpdated { get; init; }

		public ImageInfo()
		{
			Hash = "";
			LastUpdated = DateTime.MinValue;
		}
	}
}