using Newtonsoft.Json;

namespace Hosta.API.Friend
{
	public record FriendInfo
	{
		[JsonProperty(Required = Required.Always)]
		public string ID { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string Name { get; init; }

		[JsonProperty(Required = Required.Always)]
		public bool IsFavorite { get; init; }

		public FriendInfo()
		{
			ID = "";
			Name = "";
			IsFavorite = false;
		}
	}
}