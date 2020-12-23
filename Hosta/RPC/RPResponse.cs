using System;
using Hosta.RPC;
using Newtonsoft.Json;

namespace Hosta.RPC
{
	/// <summary>
	/// Represents a response to a remote procedure call.
	/// </summary>
	public record RPResponse
	{
		public RPResponse()
		{
			ReturnValues = "";
		}

		[JsonProperty(Required = Required.Always)]
		public Guid ID { get; init; }

		[JsonProperty(Required = Required.Always)]
		public bool Success { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string ReturnValues { get; init; }
	}
}