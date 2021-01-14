using Hosta.API.Profile;
using Hosta.API.Image;
using Hosta.Crypto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hosta.API
{
	public abstract class API
	{
		public abstract Task<string> AddImage(AddImageRequest request, PublicIdentity client = null);

		/// <summary>
		/// Gets a resource from the server.
		/// </summary>
		public abstract Task<GetImageResponse> GetImage(string hash, PublicIdentity client = null);

		/// <summary>
		/// Gets a list of all images.
		/// </summary>
		public abstract Task<List<ImageInfo>> GetImageList(PublicIdentity client = null);

		/// <summary>
		/// Gets the profile.
		/// </summary>
		public abstract Task<GetProfileResponse> GetProfile(PublicIdentity client = null);

		/// <summary>
		/// Removes an image.
		/// </summary>
		public abstract Task RemoveImage(string hash, PublicIdentity client = null);

		/// <summary>
		/// Sets the profile.
		/// </summary>
		public abstract Task SetProfile(SetProfileRequest request, PublicIdentity client = null);

		//// Translation

		private readonly static JsonSerializerSettings settings = new()
		{
			MissingMemberHandling = MissingMemberHandling.Error
		};

		/// <summary>
		/// Imports an item from a JSON string.
		/// </summary>
		public static T Import<T>(string json)
		{
			var imported = JsonConvert.DeserializeObject<T>(json, settings);
			if (imported is null) throw new Exception($"Could not import {typeof(T)}!");
			return imported;
		}

		/// <summary>
		/// Exports an item to a JSON string.
		/// </summary>
		public static string Export<T>(T item)
		{
			return JsonConvert.SerializeObject(item);
		}
	}
}