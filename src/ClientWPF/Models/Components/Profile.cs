using Hosta.API.Data;
using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			DisplayName = "[]";
			ID = "[]";
			Tagline = "[]";
			Bio = "[]";
			Avatar = Array.Empty<byte>();
			LastUpdated = "[]";
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