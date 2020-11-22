using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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