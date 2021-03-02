using System;

namespace Hosta.Tools
{
	public static class Guard
	{
		public static T GuardNull<T>(this T? item) => item ?? throw new NullReferenceException();
	}
}