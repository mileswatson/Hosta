using System;

namespace Hosta.RPC
{
	/// <summary>
	/// Represents a response to a remote procedure call.
	/// </summary>
	public record RPResponse
	{
		public Guid ID { get; init; }
		public bool Success { get; init; }
		public string ReturnValues { get; init; }
	}
}