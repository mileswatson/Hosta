using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.RPC;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Hosta.API
{
	/// <summary>
	/// Used to make requests to a remote API.
	/// </summary>
	public class APITranslatorClient : API, IDisposable
	{
		/// <summary>
		/// Underlying RP Client to use.
		/// </summary>
		private readonly RPClient client;

		/// <summary>
		/// Creates a new instance of a RemoteAPIGateway.
		/// </summary>
		private APITranslatorClient(RPClient client)
		{
			this.client = client;
		}

		/// <summary>
		/// Connects an RPClient to the given endpoint, and then constructs a RemoteAPIGateway from it.
		/// </summary>
		public static async Task<APITranslatorClient> CreateAndConnect(ConnectionArgs args)
		{
			var client = await RPClient.CreateAndConnect(
				args.ServerID,
				new IPEndPoint(args.Address, args.Port),
				args.Self ?? throw new NullReferenceException("Self should not be null!")
			).ConfigureAwait(false);
			return new APITranslatorClient(client);
		}

		public record ConnectionArgs
		{
			public string ServerID { get; init; }

			public IPAddress Address { get; init; }

			public int Port { get; init; }

			public PrivateIdentity? Self { get; init; }

			public ConnectionArgs()
			{
				ServerID = "";
				Address = IPAddress.None;
			}
		}

		/// <summary>
		/// Disposes if any critical exceptions are thrown.
		/// </summary>
		private async Task<string> Call(string procedure, string args)
		{
			try
			{
				return await client.Call(procedure, args);
			}
			catch (Exception e) when (e is not RPException)
			{
				Dispose();
				throw;
			}
			catch (RPException e)
			{
				throw new APIException(e.Message);
			}
		}

		//// Translators

		public override async Task<string> AddImage(AddImageRequest request, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			return await Call(nameof(AddImage), Export(request));
		}

		public override async Task<GetImageResponse> GetImage(string hash, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			var str = await Call(nameof(GetImage), hash);
			return Import<GetImageResponse>(str);
		}

		public override async Task<List<ImageInfo>> GetImageList(PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			var str = await Call(nameof(GetImageList), "");
			return Import<List<ImageInfo>>(str);
		}

		public override async Task RemoveImage(string hash, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			await Call(nameof(RemoveImage), hash);
		}

		public override async Task<string> AddPost(AddPostRequest request, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			return await Call(nameof(AddPost), Export(request));
		}

		public override async Task<GetPostResponse> GetPost(string id, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			var str = await Call(nameof(GetPost), id);
			return Import<GetPostResponse>(str);
		}

		public override async Task<List<PostInfo>> GetPostList(DateTimeOffset start, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			var str = await Call(nameof(GetPostList), Export(start));
			return Import<List<PostInfo>>(str);
		}

		public override async Task<GetProfileResponse> GetProfile(PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			var str = await Call(nameof(GetProfile), "");
			return Import<GetProfileResponse>(str);
		}

		public override async Task RemovePost(string id, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			await Call(nameof(RemovePost), id);
		}

		public override Task SetProfile(SetProfileRequest request, PublicIdentity? _ = null)
		{
			ThrowIfDisposed();
			return Call(nameof(SetProfile), Export(request));
		}

		//// Implements IDisposable

		private bool disposed = false;

		public bool Disposed
		{
			get => disposed;
		}

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

			if (disposing)
			{
				client.Dispose();
			}

			disposed = true;
		}
	}
}