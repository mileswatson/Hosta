using Newtonsoft.Json;

namespace Hosta.API.Address
{
	public record AddressInfo
	{
		[JsonProperty(Required = Required.Always)]
		public string IP { get; init; }

		[JsonProperty(Required = Required.Always)]
		public int Port { get; init; }

		public AddressInfo()
		{
			IP = "";
			Port = -1;
		}
	}
}