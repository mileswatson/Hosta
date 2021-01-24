using Hosta.API;
using Hosta.API.Address;
using Hosta.Crypto;
using Node.Users;
using SQLite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Node.Addresses
{
	internal class AddressHandler
	{
		readonly private SQLiteAsyncConnection conn;

		readonly private UserHandler users;

		readonly private PrivateIdentity tempIdentity = PrivateIdentity.Create();

		private AddressHandler(SQLiteAsyncConnection conn, UserHandler users)
		{
			this.conn = conn;
			this.users = users;
		}

		public static async Task<AddressHandler> Create(SQLiteAsyncConnection conn, UserHandler users)
		{
			await conn.CreateTableAsync<Address>();
			return new AddressHandler(conn, users);
		}

		private async Task VerifyAddress(string user, IPAddress address, int port)
		{
			var args = new APITranslatorClient.ConnectionArgs
			{
				Address = address,
				Port = port,
				Self = tempIdentity,
				ServerID = user
			};
			try
			{
				var connection = await APITranslatorClient.CreateAndConnect(args);
				connection.Dispose();
			}
			catch
			{
				throw new APIException("Could not connect...");
			}
		}

		public async Task AddAddress(string user, IPAddress address, int port, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Self);

			await AddAddress(user, address, port);
		}

		private async Task AddAddress(string user, IPAddress address, int port)
		{
			await VerifyAddress(user, address, port);

			await conn.InsertOrReplaceAsync(new Address
			{
				UserID = user,
				IP = address.ToString(),
				Port = port,
			});
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

			await AddAddress(client.ID, address, port);
		}
	}
}