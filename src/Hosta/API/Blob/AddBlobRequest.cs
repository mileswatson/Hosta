using Newtonsoft.Json;
using System;

namespace Hosta.API.Data
{
	/// <summary>
	/// Represents the request for an AddResource operation.
	/// </summary>
	public record AddBlobRequest
	{
		[JsonProperty(Required = Required.Always)]
		public byte[] Data { get; init; }

		public AddBlobRequest()
		{
			Data = Array.Empty<byte>();
		}
	}
}