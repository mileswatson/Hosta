using ClientWPF.Models.Data;
using Hosta.API;
using Hosta.API.Data;
using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.IO;
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

		private readonly AsyncCache<Profile> Profiles = new AsyncCache<Profile>(
			(Task<Profile> t) => true,
			(Task<Profile> t) => { }
		);

		private readonly AsyncCache<BitmapImage> Images = new AsyncCache<BitmapImage>(
			(Task<BitmapImage> t) => true,
			(Task<BitmapImage> t) => { }
		);

		/// <summary>
		/// Creates a new instance of a ResourceManager.
		/// </summary>
		public ResourceManager(PrivateIdentity self, RemoteAPIGateway node, Action onConnectionFail)
		{
			Self = self.ID;
			connections = new ConnectionManager(self, node, () =>
			{
				Dispose();
				onConnectionFail();
			});
		}

		//// Implementation

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

		public async Task<string> AddImage(string user, byte[] data, bool force = false)
		{
			ThrowIfDisposed();
			var request = new AddImageRequest
			{
				Data = data
			};
			var conn = await connections.GetConnection(Self);
			return await conn.AddImage(request);
		}

		//// Static

		public static ResourceManager? Resources { get; set; }

		static ResourceManager()
		{
			var image = new BitmapImage();
			image.BeginInit();
			image.DecodePixelWidth = 160;
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = new Uri("Assets/Images/default-avatar.png", UriKind.Relative);
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