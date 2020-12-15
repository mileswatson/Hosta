using System.Threading.Tasks;

namespace Hosta.RPC
{
	/// <summary>
	/// Allows an RPServer to interface with external code.
	/// </summary>
	public interface ICallable
	{
		public Task<string> Call(string procedure, string args);
	}
}