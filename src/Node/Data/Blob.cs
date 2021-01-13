using Hosta.API.Data;
using Hosta.Tools;
using SQLite;
using System;
using System.Security.Cryptography;

namespace Node.Data
{
	internal record Blob
	{
		[PrimaryKey]
		public string Hash { get; init; }

		public string Title { get; init; }

		public byte[] Data { get; init; }

		public DateTime LastUpdated { get; init; }

		public Blob()
		{
			Hash = "";
			Title = "";
			Data = Array.Empty<byte>();
			LastUpdated = DateTime.MinValue;
		}

		public GetBlobResponse ToResponse() => new GetBlobResponse
		{
			Data = Data,
			LastUpdated = LastUpdated
		};

		public static Blob FromAddRequest(AddBlobRequest request) => new Blob
		{
			Hash = Transcoder.HexFromBytes(SHA256.HashData(request.Data)),
			Data = request.Data,
			LastUpdated = DateTime.Now
		};
	}
}