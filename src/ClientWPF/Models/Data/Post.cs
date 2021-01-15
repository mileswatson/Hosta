using System;

namespace ClientWPF.Models.Data
{
	public record Post
	{
		public string UserID { get; init; }

		public string PostID { get; init; }

		public string Content { get; init; }

		public string FileName { get; init; }

		public byte[] FileData { get; init; }

		public DateTime LastUpdated { get; init; }

		public Post()
		{
			UserID = "[ID]";
			PostID = "[PostID]";
			Content = "[Content]";
			FileName = "[FileName]";
			FileData = Array.Empty<byte>();
			LastUpdated = DateTime.MinValue;
		}
	}
}