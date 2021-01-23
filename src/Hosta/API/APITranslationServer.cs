using Hosta.API.Friend;
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
	/// Translates RPC calls to API calls.
	/// </summary>
	public class APITranslationServer : ICallable, IDisposable
	{
		/// <summary>
		/// Underlying API to call.
		/// </summary>
		private readonly API api;

		/// <summary>
		/// Underlying RPServer to receive calls from.
		/// </summary>
		private readonly RPServer server;

		/// <summary>
		/// Creates a new instance of a LocalAPIGateway.
		/// </summary>
		public APITranslationServer(PrivateIdentity self, IPEndPoint endPoint, API gateway)
		{
			server = new RPServer(self, endPoint, this);
			this.api = gateway;
		}

		/// <summary>
		/// Handles an RP call.
		/// </summary>
		public async Task<string> Call(string proc, string args, PublicIdentity client, IPEndPoint address)
		{
			if (client is null) throw new Exception("Unknown identity!");

			try
			{
				// Decides which handler to run.
				Task<string> result = proc switch
				{
					nameof(GetAddresses) => GetAddresses(args, client),
					nameof(InformAddress) => InformAddress(client, address),
					nameof(GetFriendList) => GetFriendList(client),
					nameof(RemoveFriend) => RemoveFriend(args, client),
					nameof(SetFriend) => SetFriend(args, client),
					nameof(AddImage) => AddImage(args, client),
					nameof(GetImage) => GetImage(args, client),
					nameof(GetImageList) => GetImageList(client),
					nameof(RemoveImage) => RemoveImage(args, client),
					nameof(AddPost) => AddPost(args, client),
					nameof(GetPost) => GetPost(args, client),
					nameof(GetPostList) => GetPostList(args, client),
					nameof(RemovePost) => RemovePost(args, client),
					nameof(GetProfile) => GetProfile(args, client),
					nameof(SetProfile) => SetProfile(args, client),
					_ => throw new Exception("Invalid procedure!"),
				};
				return await result;
			}
			catch (APIException e)
			{
				throw new RPException(e.Message);
			}
		}

		/// <summary>
		/// Starts the RPServer.
		/// </summary>
		public Task Run()
		{
			return server.ListenForClients();
		}

		//// Translators

		public async Task<string> GetAddresses(string args, PublicIdentity client)
		{
			var request = API.Import<List<string>>(args);
			var response = await api.GetAddresses(request, client);
			return API.Export(response);
		}

		public async Task<string> InformAddress(PublicIdentity client, IPEndPoint address)
		{
			await api.InformAddress(address, client);
			return "";
		}

		public async Task<string> GetFriendList(PublicIdentity client)
		{
			var response = await api.GetFriendList(client);
			return API.Export(response);
		}

		public async Task<string> RemoveFriend(string args, PublicIdentity client)
		{
			await api.RemoveFriend(args, client);
			return "";
		}

		public async Task<string> SetFriend(string args, PublicIdentity client)
		{
			FriendInfo info;
			try
			{
				info = API.Import<FriendInfo>(args);
			}
			catch
			{
				throw new RPException("Arguments were formatted incorrectly!");
			}
			await api.SetFriend(info, client);
			return "";
		}

		public async Task<string> AddImage(string args, PublicIdentity client)
		{
			AddImageRequest r;
			try
			{
				r = API.Import<AddImageRequest>(args);
			}
			catch
			{
				throw new RPException("Arguments were formatted incorrectly!");
			}
			return await api.AddImage(r, client);
		}

		public async Task<string> GetImage(string args, PublicIdentity client)
		{
			var response = await api.GetImage(args, client);
			return API.Export(response);
		}

		public async Task<string> GetImageList(PublicIdentity client)
		{
			var response = await api.GetImageList(client);
			return API.Export(response);
		}

		public async Task<string> RemoveImage(string args, PublicIdentity client)
		{
			await api.RemoveImage(args, client);
			return "";
		}

		public async Task<string> AddPost(string args, PublicIdentity client)
		{
			AddPostRequest r;
			try
			{
				r = API.Import<AddPostRequest>(args);
			}
			catch
			{
				throw new RPException("Arguments were formatted incorrectly!");
			}
			return await api.AddPost(r, client);
		}

		public async Task<string> GetPost(string args, PublicIdentity client)
		{
			var response = await api.GetPost(args, client);
			return API.Export(response);
		}

		public async Task<string> GetPostList(string args, PublicIdentity client)
		{
			DateTimeOffset start;
			try
			{
				start = API.Import<DateTimeOffset>(args);
			}
			catch
			{
				throw new RPException("Arguments were formatted incorrectly!");
			}
			var response = await api.GetPostList(start, client);
			return API.Export(response);
		}

		public async Task<string> RemovePost(string args, PublicIdentity client)
		{
			await api.RemovePost(args, client);
			return "";
		}

		public async Task<string> GetProfile(string args, PublicIdentity client)
		{
			if (args != "") throw new RPException("Arguments were formatted incorrectly!");
			var response = await api.GetProfile(client);
			return API.Export(response);
		}

		public async Task<string> SetProfile(string args, PublicIdentity client)
		{
			SetProfileRequest r;
			try
			{
				r = API.Import<SetProfileRequest>(args);
			}
			catch
			{
				throw new RPException("Arguments were formatted incorrectly!");
			}
			await api.SetProfile(r, client);
			return "";
		}

		//// Implements IDisposable

		private bool disposed = false;

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
				server.Dispose();
			}

			disposed = true;
		}
	}
}