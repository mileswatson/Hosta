using Hosta.API.Data;
using Hosta.Tools;
using SQLite;
using System;
using System.Security.Cryptography;

namespace Node.Data
{
	internal record Resource
	{
		[PrimaryKey]
		public string Hash { get; init; }

		public byte[] Data { get; init; }

		public DateTime LastUpdated { get; init; }

		public Resource()
		{
			Hash = "";
			Data = Array.Empty<byte>();
			LastUpdated = DateTime.MinValue;
		}

		public GetResourceResponse ToResponse() => new GetResourceResponse
		{
			Data = Data,
			LastUpdated = LastUpdated
		};

		public static Resource FromAddRequest(AddResourceRequest request) => new Resource
		{
			Hash = Transcoder.HexFromBytes(SHA256.HashData(request.Data)),
			Data = request.Data,
			LastUpdated = DateTime.Now
		};
	}
}