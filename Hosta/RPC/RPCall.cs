using Newtonsoft.Json;
using System;

namespace Hosta.RPC
{
	/// <summary>
	/// Represents a remote procedure call.
	/// </summary>
	public record RPCall
	{
		[JsonProperty(Required = Required.Always)]
		public Guid ID { get; init; }

		private string procedure = "";

		[JsonProperty(Required = Required.Always)]
		public string Procedure
		{
			get
			{
				return procedure;
			}
			init
			{
				procedure = value;
			}
		}

		private string procedureArgs = "";

		[JsonProperty(Required = Required.Always)]
		public string ProcedureArgs
		{
			get
			{
				return procedureArgs;
			}

			init
			{
				procedureArgs = value;
			}
		}
	}
}