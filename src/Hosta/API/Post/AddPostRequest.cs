using Newtonsoft.Json;

namespace Hosta.API.Post
{
	public record AddPostRequest
	{
		[JsonProperty(Required = Required.Always)]
		public string Content { get; init; }

		[JsonProperty(Required = Required.Always)]
		public string ImageHash { get; init; }

		public AddPostRequest()
		{
			Content = "";
			ImageHash = "";
		}
	}
}