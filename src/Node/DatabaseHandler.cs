using SQLite;

namespace Node
{
	internal class DatabaseHandler
	{
		protected readonly SQLiteAsyncConnection conn;

		protected readonly string self;

		protected DatabaseHandler(SQLiteAsyncConnection conn, string self)
		{
			this.conn = conn;
			this.self = self;
		}
	}
}