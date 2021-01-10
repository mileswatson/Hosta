using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClientWPF.ViewModels
{
	public static class Tools
	{
		public static BitmapImage TryGetImage(byte[] data)
		{
			try
			{
				return GetImage(data);
			}
			catch
			{
				return DefaultImage;
			}
		}

		public static BitmapImage GetImage(byte[] data)
		{
			var image = new BitmapImage();
			using (var stream = new MemoryStream(data, 0, data.Length))
			{
				image.BeginInit();
				image.DecodePixelWidth = 160;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = stream;
				image.EndInit();
			}
			return image;
		}

		public static BitmapImage DefaultImage
		{
			get
			{
				var image = new BitmapImage();
				image.BeginInit();
				image.DecodePixelWidth = 160;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = new Uri("Assets/Images/default-avatar.png", UriKind.Relative);
				image.EndInit();
				return image;
			}
		}
	}
}