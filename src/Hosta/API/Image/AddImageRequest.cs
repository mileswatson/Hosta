using Newtonsoft.Json;
using System;

namespace Hosta.API.Image
{
	/// <summary>
	/// Represents the request for an AddResource operation.
	/// </summary>
	public record AddImageRequest
	{
		[JsonProperty(Required = Required.Always)]
		public byte[] Data { get; init; }

		public AddImageRequest()
		{
			Data = Array.Empty<byte>();
		}
	}
}