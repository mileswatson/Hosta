using Hosta.API.Friend;
using Hosta.API.Image;
using Hosta.API.Post;
using Hosta.API.Profile;
using Hosta.Crypto;
using Hosta.RPC;
using System;
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
		public async Task<string> Call(string proc, string args, PublicIdentity client)
		{
			if (client is null) throw new Exception("Unknown identity!");

			// Decides which handler to use.
			ProcedureHandler handler = proc switch
			{
				nameof(GetFriendList) => GetFriendList,
				nameof(RemoveFriend) => RemoveFriend,
				nameof(SetFriend) => SetFriend,
				nameof(AddImage) => AddImage,
				nameof(GetImage) => GetImage,
				nameof(GetImageList) => GetImageList,
				nameof(RemoveImage) => RemoveImage,
				nameof(AddPost) => AddPost,
				nameof(GetPost) => GetPost,
				nameof(GetPostList) => GetPostList,
				nameof(RemovePost) => RemovePost,
				nameof(GetProfile) => GetProfile,
				nameof(SetProfile) => SetProfile,
				_ => throw new Exception("Invalid procedure!"),
			};
			try
			{
				return await handler(args, client);
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

		/// <summary>
		/// Represents an RPC to API translator.
		/// </summary>
		private delegate Task<string> ProcedureHandler(string args, PublicIdentity client);

		//// Translators

		public async Task<string> GetFriendList(string _, PublicIdentity client)
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

		public async Task<string> GetImageList(string _, PublicIdentity client)
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