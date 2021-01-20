using Newtonsoft.Json;
using System;

namespace Hosta.API.Post
{
	public record PostInfo
	{
		[JsonProperty(Required = Required.Always)]
		public string ID { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTimeOffset TimePosted { get; init; }

		public PostInfo()
		{
			ID = "";
			TimePosted = DateTimeOffset.MinValue;
		}
	}
}