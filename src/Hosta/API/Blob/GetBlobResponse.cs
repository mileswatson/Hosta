using Newtonsoft.Json;
using System;

namespace Hosta.API.Data
{
	public record GetBlobResponse
	{
		[JsonProperty(Required = Required.Always)]
		public byte[] Data { get; init; }

		[JsonProperty(Required = Required.Always)]
		public DateTime LastUpdated { get; init; }

		public GetBlobResponse()
		{
			Data = Array.Empty<byte>();
			LastUpdated = DateTime.MinValue;
		}
	}
}