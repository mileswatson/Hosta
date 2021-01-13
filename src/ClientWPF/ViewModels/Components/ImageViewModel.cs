using System.Windows.Media.Imaging;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.Components
{
	public class ImageViewModel : ObservableObject
	{
		private BitmapImage _image = DefaultImage;

		public BitmapImage Image
		{
			get => _image;
			private set
			{
				_image = value;
				NotifyPropertyChanged(nameof(Image));
			}
		}

		public string User { get; init; }
		public string Hash { get; init; }

		public ImageViewModel()
		{
			User = "";
			Hash = "";
		}

		public ImageViewModel(string user, string hash)
		{
			User = user;
			Hash = hash;
		}

		public override async void Update(bool force = false)
		{
			if (User == "" || Hash == "") return;
			var newImage = await Resources!.GetImage(User, Hash, force);
			if (Image != newImage) Image = newImage;
		}
	}
}