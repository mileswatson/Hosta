using Hosta.API.Data;
using Hosta.Crypto;
using System.Threading.Tasks;

namespace Hosta.API
{
	public abstract class API
	{
		/// <summary>
		/// Gets the readable name of the client.
		/// </summary>
		public abstract Task<GetProfileResponse> GetProfile(PublicIdentity client = null);

		/// <summary>
		/// Sets the readable name of the client.
		/// </summary>
		public abstract Task SetProfile(SetProfileRequest profile, PublicIdentity client = null);
	}
}