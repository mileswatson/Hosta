using Hosta.API.Post;
using Hosta.Crypto;
using Hosta.Tools;
using SQLite;
using System;

namespace Node.Posts
{
	internal record Post
	{
		[PrimaryKey]
		public string ID { get; init; }

		public string Content { get; init; }

		public string ImageHash { get; init; }

		public DateTime TimePosted { get; init; }

		public Post()
		{
			ID = "";
			Content = "";
			ImageHash = "";
			TimePosted = DateTime.MinValue;
		}

		public GetPostResponse ToResponse() => new GetPostResponse
		{
			Content = Content,
			ImageHash = ImageHash,
			TimePosted = TimePosted
		};

		public static Post FromAddRequest(AddPostRequest request) => new Post
		{
			ID = Transcoder.HexFromBytes(SecureRandomGenerator.GetBytes(32)),
			Content = request.Content,
			ImageHash = request.ImageHash,
			TimePosted = DateTime.Now
		};
	}
}