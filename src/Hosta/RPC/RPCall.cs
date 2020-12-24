using Newtonsoft.Json;
using System;

namespace Hosta.RPC
{
	/// <summary>
	/// Represents a remote procedure call.
	/// </summary>
	public record RPCall
	{
		public RPCall()
		{
			Procedure = "";
			ProcedureArgs = "";
		}

		[JsonProperty(Required = Required.Always)]
		public Guid ID { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Procedure { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string ProcedureArgs { get; init; }
	}
}