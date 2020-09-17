using System;
using System.Collections.Generic;
using System.Text;

namespace Hosta.Tools
{
	internal class Debug
	{
		public static void Log(object s)
		{
			Console.WriteLine(s.ToString());
		}
	}
}