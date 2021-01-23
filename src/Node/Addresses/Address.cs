using SQLite;

namespace Node.Addresses
{
	internal record Address
	{
		[PrimaryKey]
		public string UserID { get; init; }

		public string IP { get; init; }

		public int Port { get; init; }

		public Address()
		{
			UserID = "";
			IP = "";
			Port = 0;
		}
	}
}