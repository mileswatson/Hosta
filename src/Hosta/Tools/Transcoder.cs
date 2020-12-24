using System;
using System.Text;

namespace Hosta.Tools
{
	/// <summary>
	/// Used to transcode between datatypes.
	/// </summary>
	public static class Transcoder
	{
		/// <summary>
		/// Encodes a string using UTF8.
		/// </summary>
		public static byte[] BytesFromText(string text)
		{
			return Encoding.UTF8.GetBytes(text);
		}

		/// <summary>
		/// Decodes UTF8 encoded bytes.
		/// </summary>
		public static string TextFromBytes(byte[] utf8bytes)
		{
			var enc = new UTF8Encoding(false, true);

			try
			{
				return enc.GetString(utf8bytes);
			}
			catch
			{
				throw new FormatException("UTF8 string contained invalid characters!");
			}
		}

		/// <summary>
		/// Encodes bytes as a hex string.
		/// </summary>
		public static string HexFromBytes(byte[] bytes)
		{
			StringBuilder hex = new StringBuilder(bytes.Length * 2);
			foreach (byte b in bytes)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		/// <summary>
		/// Decodes a hex string to bytes.
		/// </summary>
		public static byte[] BytesFromHex(string hex)
		{
			if (hex.Length % 2 != 0)
			{
				throw new FormatException("Hex string contained an even number of characters!");
			}
			try
			{
				int NumberChars = hex.Length;
				byte[] bytes = new byte[NumberChars / 2];
				for (int i = 0; i < NumberChars; i += 2)
					bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
				return bytes;
			}
			catch
			{
				throw new FormatException("Hex string contained invalid characters!");
			}
		}
	}
}