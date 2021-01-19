using System;

namespace Hosta.API
{
	public class APIException : Exception
	{
		public APIException(string message) : base(message)
		{
		}
	}
}