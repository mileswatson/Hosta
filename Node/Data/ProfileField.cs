using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Node.Data
{
	internal record ProfileField
	{
		public ProfileField()
		{
			Key = "";
			Value = "";
		}

		public ProfileField(string key, string value)
		{
			Key = key;
			Value = value;
		}

		[PrimaryKey]
		public string Key { get; init; }

		public string Value { get; init; }
	}
}