using Newtonsoft.Json;
using System;

namespace Hosta.API.Data
{
	/// <summary>
	/// Represents the request for an AddResource operation.
	/// </summary>
	public record AddResourceRequest
	{
		[JsonProperty(Required = Required.Always)]
		public byte[] Data { get; init; }

		public AddResourceRequest()
		{
			Data = Array.Empty<byte>();
		}
	}
}