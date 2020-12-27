using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hosta.RPC
{
	public class RPException : Exception
	{
		public RPException(string message) : base(message)
		{
		}
	}
}