﻿using SQLite;
using Hosta.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Node.Data
{
	/// <summary>
	/// Represents a user profile.
	/// </summary>
	internal record Profile
	{
		public Profile() =>
			(ID, DisplayName, Tagline, Bio, Avatar, LastUpdated)
			= ("", "", "", "", "", DateTime.UtcNow);

		public Profile(string id, string displayName, string tagline, string bio, string avatar, DateTime lastUpdated) =>
			   (ID, DisplayName, Tagline, Bio, Avatar, LastUpdated)
			 = (id, displayName, tagline, bio, avatar, lastUpdated);

		public Profile(string id, SetProfileRequest r) : this(id, r.DisplayName, r.Tagline, r.Bio, r.Avatar, DateTime.UtcNow) { }

		public GetProfileResponse ToResponse()
		{
			return new GetProfileResponse(ID, DisplayName, Tagline, Bio, Avatar, LastUpdated);
		}

		[PrimaryKey]
		public string ID { get; init; }

		public string DisplayName { get; init; }

		public string Tagline { get; init; }

		public string Bio { get; init; }

		public string Avatar { get; init; }

		public DateTime LastUpdated { get; init; }
	}
}