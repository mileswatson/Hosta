using Newtonsoft.Json;
using System;

namespace Hosta.API.Data
{
	public record GetResourceResponse
	{
		[JsonProperty(Required = Required.Always)]
		public byte[] Data { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTime LastUpdated { get; init; }

		public GetResourceResponse()
		{
			Data = Array.Empty<byte>();
			LastUpdated = DateTime.MinValue;
		}
	}
}