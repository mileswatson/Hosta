using Hosta.API.Post;
using System;

namespace ClientWPF.Models.Data
{
	public record Post
	{
		public string User { get; init; }

		public string ID { get; init; }

		public string Content { get; init; }

		public string ImageHash { get; init; }

		public DateTime TimePosted { get; init; }

		public Post()
		{
			User = "";
			ID = "";
			Content = "";
			ImageHash = "";
			TimePosted = DateTime.MinValue;
		}

		public static Post FromResponse(GetPostResponse response, string user, string id) => new Post
		{
			User = user,
			ID = id,
			Content = response.Content,
			ImageHash = response.ImageHash,
			TimePosted = response.TimePosted.LocalDateTime
		};
	}
}