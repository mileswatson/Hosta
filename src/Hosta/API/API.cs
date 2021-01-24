using Hosta.API.Address;
using Hosta.API.Friend;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.API
{
	public abstract class API
	{
		/// <summary>
		/// Manually adds the address of a friend.
		/// </summary>
		public abstract Task AddAddress(Tuple<string, AddressInfo> address, PublicIdentity client = null);

		/// <summary>
		/// Gets the addresses that the server knows from the list.
		/// </summary>
		public abstract Task<Dictionary<string, AddressInfo>> GetAddresses(List<string> users, PublicIdentity client = null);

		/// <summary>
		/// Informs the node of the client node's address.
		/// </summary>
		public abstract Task InformAddress(int port, IPAddress address = null, PublicIdentity client = null);

		/// <summary>
		/// Gets a list of the user's friends from the server.
		/// </summary>
		public abstract Task<List<FriendInfo>> GetFriendList(PublicIdentity client = null);

		/// <summary>
		/// Removes a friend.
		/// </summary>
		public abstract Task RemoveFriend(string user, PublicIdentity client = null);

		/// <summary>
		/// Adds a friend, or updates it if the friend already exists.
		/// </summary>
		public abstract Task SetFriend(FriendInfo info, PublicIdentity client = null);

		/// <summary>
		/// Add an image to the image library.
		/// </summary>
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
		/// Removes an image.
		/// </summary>
		public abstract Task RemoveImage(string hash, PublicIdentity client = null);

		/// <summary>
		/// Adds a post.
		/// </summary>
		public abstract Task<string> AddPost(AddPostRequest request, PublicIdentity client = null);

		/// <summary>
		/// Retrieves the post with the given id.
		/// </summary>
		public abstract Task<GetPostResponse> GetPost(string id, PublicIdentity client = null);

		/// <summary>
		/// Gets a list of the posts that were added after the given DateTimeOffset.
		/// </summary>
		public abstract Task<List<PostInfo>> GetPostList(DateTimeOffset start, PublicIdentity client = null);

		/// <summary>
		/// Removes the post with the given ID.
		/// </summary>
		public abstract Task RemovePost(string id, PublicIdentity client = null);

		/// <summary>
		/// Gets the profile.
		/// </summary>
		public abstract Task<GetProfileResponse> GetProfile(PublicIdentity client = null);

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