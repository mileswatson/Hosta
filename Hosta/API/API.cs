using Hosta.Crypto;
using System.Threading.Tasks;

namespace Hosta.API
{
	public abstract class API
	{
		/// <summary>
		/// Returns the readable name of the client.
		/// </summary>
		public abstract Task<string> Name(PublicIdentity client = null);
	}
}