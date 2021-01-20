using Newtonsoft.Json;
using System;

namespace Hosta.API.Post
{
	public record GetPostResponse
	{
		[JsonProperty(Required = Required.Always)]
		public string Content { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string ImageHash { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTimeOffset TimePosted { get; init; }

		public GetPostResponse()
		{
			Content = "";
			ImageHash = "";
			TimePosted = DateTimeOffset.MinValue;
		}
	}
}