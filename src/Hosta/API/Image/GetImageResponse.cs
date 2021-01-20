using Newtonsoft.Json;
using System;

namespace Hosta.API.Image
{
	public record GetImageResponse
	{
		[JsonProperty(Required = Required.Always)]
		public byte[] Data { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTimeOffset LastUpdated { get; init; }

		public GetImageResponse()
		{
			Data = Array.Empty<byte>();
			LastUpdated = DateTimeOffset.MinValue;
		}
	}
}