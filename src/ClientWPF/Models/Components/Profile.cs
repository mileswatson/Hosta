using Hosta.API.Data;
using Hosta.Tools;
using System;

namespace ClientWPF.Models.Components
{
	public record Profile
	{
		public string DisplayName { get; init; }

		public string ID { get; init; }

		public string Tagline { get; init; }

		public string Bio { get; init; }

		public byte[] Avatar { get; init; }

		public string LastUpdated { get; init; }

		public Profile()
		{
			DisplayName = "[Name]";
			ID = "[ID]";
			Tagline = "[Tagline]";
			Bio = "[Bio]";
			Avatar = Array.Empty<byte>();
			LastUpdated = "[LastUpdated]";
		}

		public Profile(GetProfileResponse response)
		{
			DisplayName = response.DisplayName;
			ID = response.ID;
			Tagline = response.Tagline;
			Bio = response.Bio;
			LastUpdated = response.LastUpdated.ToShortDateString();
			try
			{
				Avatar = Transcoder.BytesFromHex(response.Avatar);
			}
			catch
			{
				Avatar = Array.Empty<byte>();
			}
		}
	}
}