﻿using ClientWPF.Models.Data;
using Hosta.API;
using Hosta.API.Address;
using Hosta.API.Friend;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClientWPF.Models
{
	/// <summary>
	/// Handles network resources.
	/// </summary>
	public class ResourceManager : IDisposable
	{
		public string Self { get; init; }

		private readonly ConnectionManager connections;

		private readonly AsyncCache<Profile> Profiles = new();

		private readonly AsyncCache<BitmapImage> Images = new();

		private readonly AsyncCache<Post> Posts = new();

		/// <summary>
		/// Creates a new instance of a ResourceManager.
		/// </summary>
		public ResourceManager(PrivateIdentity self, APITranslatorClient node, Action onConnectionFail)
		{
			Self = self.ID;
			connections = new ConnectionManager(self, node, () =>
			{
				Dispose();
				onConnectionFail();
			});
		}

		//// Implementation

		public async Task UpdateAddress(string user)
		{
			await connections.GetConnection(user, true);
		}

		public async Task AddAddress(string user, IPAddress address, int port)
		{
			var conn = await connections.GetConnection(Self);
			var request = new AddressInfo
			{
				IP = address.ToString(),
				Port = port
			};
			await conn.AddAddress(new(user, request));
			await UpdateAddress(user);
		}

		public async Task<List<FriendInfo>> GetFriendList()
		{
			var conn = await connections.GetConnection(Self);
			return await conn.GetFriendList();
		}

		public async Task SetFriend(string user, string name, bool favorite)
		{
			var conn = await connections.GetConnection(Self);
			await conn.SetFriend(new FriendInfo
			{
				ID = user,
				Name = name,
				IsFavorite = favorite
			});
		}

		public async Task RemoveFriend(string user)
		{
			var conn = await connections.GetConnection(Self);
			await conn.RemoveFriend(user);
		}

		public async Task<string> AddImage(byte[] data)
		{
			ThrowIfDisposed();
			var request = new AddImageRequest
			{
				Data = data
			};
			var conn = await connections.GetConnection(Self);
			return await conn.AddImage(request);
		}

		public Task<BitmapImage> GetImage(string user, string hash, bool force = false)
		{
			ThrowIfDisposed();
			var key = Combine(user, hash);
			return Images.LazyGet(hash, async () =>
			{
				if (user == "" || hash == "") return DefaultImage;
				try
				{
					var conn = await connections.GetConnection(user);
					var response = await conn.GetImage(hash);
					return ImageFromBytes(response.Data);
				}
				catch
				{
					return DefaultImage;
				}
			}, TimeSpan.MaxValue, force);
		}

		public async Task<List<ImageInfo>> GetImageList()
		{
			var conn = await connections.GetConnection(Self);
			return await conn.GetImageList();
		}

		public async Task RemoveImage(string hash)
		{
			var conn = await connections.GetConnection(Self);
			await conn.RemoveImage(hash);
		}

		public async Task<string> AddPost(string content, string imageHash)
		{
			var conn = await connections.GetConnection(Self);
			return await conn.AddPost(new AddPostRequest { Content = content, ImageHash = imageHash });
		}

		public Task<Post> GetPost(string user, string id, bool force = false)
		{
			ThrowIfDisposed();
			var key = Combine(user, id);
			return Posts.LazyGet(key, async () =>
			{
				if (user == "" || id == "") throw new Exception();
				var conn = await connections.GetConnection(user);
				var response = await conn.GetPost(id);
				return Post.FromResponse(response, user, id);
			}, TimeSpan.FromHours(1), force);
		}

		public async Task<List<PostInfo>> GetPostList(string user, DateTime start)
		{
			ThrowIfDisposed();
			var conn = await connections.GetConnection(user);
			return await conn.GetPostList(start);
		}

		public async Task RemovePost(string id)
		{
			ThrowIfDisposed();
			var conn = await connections.GetConnection(Self);
			await conn.RemovePost(id);
		}

		public Task<Profile> GetProfile(string user, bool force = false)
		{
			ThrowIfDisposed();
			return Profiles.LazyGet(user, async () =>
			{
				var conn = await connections.GetConnection(user);
				var response = await conn.GetProfile();
				return Profile.FromResponse(response, user);
			}, TimeSpan.FromMinutes(5), force);
		}

		public async Task SetProfile(string name, string tagline, string bio, string avatarHash)
		{
			ThrowIfDisposed();
			var request = new SetProfileRequest
			{
				Name = name,
				Tagline = tagline,
				Bio = bio,
				AvatarHash = avatarHash
			};
			var conn = await connections.GetConnection(Self);
			await conn.SetProfile(request);
		}

		//// Static

		private static ResourceManager? _resources = null;

		public static ResourceManager Resources
		{
			get => _resources ?? throw new NullReferenceException();
			set => _resources = _resources is null ? value : throw new Exception($"{nameof(Resources)} has already been set!");
		}

		static ResourceManager()
		{
			var image = new BitmapImage();
			image.BeginInit();
			image.DecodePixelWidth = 160;
			image.CacheOption = BitmapCacheOption.OnLoad;
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Images/default-avatar.png");
			image.UriSource = new Uri(path, UriKind.Absolute);
			image.EndInit();
			DefaultImage = image;
		}

		/// <summary>
		/// Provides basic protection against cache attacks.
		/// </summary>
		private static string Combine(string user, string id)
		{
			using var hmac = new HMACSHA256(Transcoder.BytesFromHex(user));
			return Transcoder.HexFromBytes(hmac.ComputeHash(Transcoder.BytesFromHex(id)));
		}

		public static BitmapImage ImageFromBytes(byte[] data)
		{
			var image = new BitmapImage();
			using (var stream = new MemoryStream(data, 0, data.Length))
			{
				image.BeginInit();
				image.DecodePixelWidth = 160;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = stream;
				image.EndInit();
			}
			return image;
		}

		public static BitmapImage DefaultImage
		{
			get; private set;
		}

		//// Cleanup

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("Attempted post-disposal use!");
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			disposed = true;

			if (disposing)
			{
				connections.Dispose();
			}
		}
	}
}