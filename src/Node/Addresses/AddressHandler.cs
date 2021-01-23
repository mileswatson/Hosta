using Hosta.API.Address;
using Hosta.Crypto;
using Node.Users;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Node.Addresses
{
	internal class AddressHandler
	{
		readonly private SQLiteAsyncConnection conn;

		readonly private UserHandler users;

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

		public async Task InformAddress(IPEndPoint address, PublicIdentity client)
		{
			await users.Authenticate(client, User.Auth.Friend);

			await conn.InsertOrReplaceAsync(new Address
			{
				UserID = client.ID,
				IP = address.Address.ToString(),
				Port = address.Port
			});
		}
	}
}