using Hosta.API;
using Hosta.API.Address;
using Hosta.Crypto;
using Node.Users;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Node.Addresses
{
	internal class AddressHandler
	{
		readonly private SQLiteAsyncConnection conn;

		readonly private UserHandler users;

		readonly private int port;

		readonly private PrivateIdentity self;

		private AddressHandler(SQLiteAsyncConnection conn, UserHandler users, int port, PrivateIdentity self)
		{
			this.conn = conn;
			this.users = users;
			this.port = port;
			this.self = self;
		}

		public static async Task<AddressHandler> Create(SQLiteAsyncConnection conn, UserHandler users, int port, PrivateIdentity self)
		{
			// Create the table if it doesn't exist.
			await conn.CreateTableAsync<Address>();

			var handler = new AddressHandler(conn, users, port, self);

			var _ = handler.Bootstrap();

			return handler;
		}

		private async Task Bootstrap()
		{
			await Task.Delay(1000);

			Console.WriteLine("Preparing for bootstrap...");

			// Convert the addresses to a dictionary and clear the table.
			var addresses = (await conn.Table<Address>().ToListAsync())
				.ToDictionary(x => x.UserID, x => x);

			await conn.Table<Address>().DeleteAsync(x => true);

			var friends = (await conn.Table<User>().ToListAsync())
				.Where(x => x.AuthLevel == User.Auth.Friend || x.AuthLevel == User.Auth.Favorite);

			Console.WriteLine("Getting all missing friends...");

			// Stores all missing friends
			var missingFriends = new HashSet<string>();

			var validConnections = new List<APITranslatorClient>();

			var tests = new List<Task<APITranslatorClient?>>();
			foreach (var friend in friends)
			{
				var id = friend.UserID;
				if (!addresses.ContainsKey(id))
				{
					missingFriends.Add(id);
					continue;
				}
				tests.Add(AddIfMissing(addresses[id], missingFriends));
			}

			await Task.WhenAll(tests);

			// Gets valid connections
			var connections = tests.Where(x => x.Result is not null)
				.Select(x => x.Result)
				.ToList();

			// Gets friends from each connection.

			Console.WriteLine($"Bootstrapping {connections.Count} valid connections...");

			foreach (var connection in connections)
			{
				if (missingFriends.Count > 0) await GetFriends(connection, missingFriends);
				connection.Dispose();
			}

			Console.WriteLine("Finished bootstrap.");
		}

		private async Task GetFriends(APITranslatorClient friendConnection, HashSet<string> missingFriends)
		{
			var foundFriends = await friendConnection.GetAddresses(missingFriends.ToList());
			var tasks = new List<Task<APITranslatorClient?>>();
			foreach (var missingFriend in missingFriends)
			{
				if (!foundFriends.ContainsKey(missingFriend)) continue;
				var address = foundFriends[missingFriend];
				tasks.Add(AddIfValid(new Address
				{
					IP = address.IP,
					Port = address.Port,
					UserID = missingFriend
				}, true));
			}
			await Task.WhenAll(tasks);
			foreach (var task in tasks)
			{
				var client = task.Result;
				if (client is null) continue;
				missingFriends.Remove(client.ServerID);
				client.Dispose();
			}
		}

		private async Task<APITranslatorClient?> AddIfMissing(Address address, HashSet<string> missing)
		{
			var connection = await AddIfValid(address, true);
			if (connection is not null) return connection;
			lock (missing)
			{
				missing.Add(address.UserID);
			}
			return null;
		}

		private async Task<APITranslatorClient?> AddIfValid(Address address, bool inform = false)
		{
			APITranslatorClient messenger;
			try
			{
				messenger = await APITranslatorClient.CreateAndConnect(new APITranslatorClient.ConnectionArgs
				{
					Address = IPAddress.Parse(address.IP),
					Port = address.Port,
					ServerID = address.UserID,
					Self = self
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}

			if (inform)
			{
				try
				{
					await messenger.InformAddress(port);
				}
				catch
				{
				}
			}

			await conn.InsertOrReplaceAsync(address);
			return messenger;
		}

		public async Task AddAddress(string user, IPAddress address, int port, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			await AddAddress(user, address, port, true);
		}

		private async Task AddAddress(string user, IPAddress address, int port, bool inform)
		{
			var x = await AddIfValid(new Address
			{
				IP = address.ToString(),
				Port = port,
				UserID = user
			}, inform);
			x?.Dispose();
		}

		public async Task<Dictionary<string, AddressInfo>> GetAddresses(List<string> requested, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Friend);

			var addresses = await conn.Table<Address>().ToListAsync();

			var response = new Dictionary<string, AddressInfo>();

			foreach (var address in addresses)
			{
				if (!requested.Contains(address.UserID)) continue;
				response[address.UserID] = new AddressInfo
				{
					IP = address.IP,
					Port = address.Port
				};
			}

			return response;
		}

		public async Task InformAddress(int port, IPAddress address, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Friend);
			await AddAddress(client.ID, address, port, false);
		}
	}
}