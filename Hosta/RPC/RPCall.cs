using System;

namespace Hosta.RPC
{
	/// <summary>
	/// Represents a remote procedure call.
	/// </summary>
	public record RPCall
	{
		public Guid ID { get; init; }

		public string Procedure { get; init; }

		public string ProcedureArgs { get; init; }
	}
}