using System;
using Hosta.RPC;

namespace Hosta.RPC
{
	/// <summary>
	/// Represents a response to a remote procedure call.
	/// </summary>
	public record RPResponse
	{
		public Guid ID { get; init; }
		public bool Success { get; init; }

		private string returnValues = "";

		public string ReturnValues
		{
			get
			{
				return returnValues;
			}
			init
			{
				returnValues = value;
			}
		}
	}
}