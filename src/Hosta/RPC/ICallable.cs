using Hosta.Crypto;
using System.Threading.Tasks;

namespace Hosta.RPC
{
	/// <summary>
	/// An interface to facilitate handling of RP calls.
	/// </summary>
	public interface ICallable
	{
		public Task<string> Call(string procedure, string args, PublicIdentity client = null);
	}
}