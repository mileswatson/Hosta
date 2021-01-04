using System;

namespace Hosta.RPC
{
	public class RPException : Exception
	{
		public RPException(string message) : base(message)
		{
		}
	}
}