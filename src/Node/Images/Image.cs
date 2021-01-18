using Hosta.API.Image;
using Hosta.Tools;
using SQLite;
using System;
using System.Security.Cryptography;

namespace Node.Images
{
	internal record Image
	{
		[PrimaryKey]
		public string Hash { get; init; }

		public byte[] Data { get; init; }

		public DateTime LastUpdated { get; init; }

		public Image()
		{
			Hash = "";
			Data = Array.Empty<byte>();
			LastUpdated = DateTime.MinValue;
		}

		public GetImageResponse ToResponse() => new GetImageResponse
		{
			Data = Data,
			LastUpdated = LastUpdated
		};

		public static Image FromAddRequest(AddImageRequest request) => new Image
		{
			Hash = Transcoder.HexFromBytes(SHA256.HashData(request.Data)),
			Data = request.Data,
			LastUpdated = DateTime.Now
		};
	}
}