using Hosta.API.Image;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.Components
{
	public class ImagePickerViewModel : ObservableObject
	{
		private List<ImageViewModel> _images = new();

		public List<ImageViewModel> Images
		{
			get => _images;
			set
			{
				_images = value;
				NotifyPropertyChanged(nameof(Images));
			}
		}

		public ICommand Select { get; init; }

		public ImagePickerViewModel(Action<string> select)
		{
			Select = new RelayCommand((object? o) =>
			{
				select((string)(o ?? ""));
			});
		}

		public override async Task UpdateAsync(bool force)
		{
			var infoList = await Resources!.GetImageList();
			infoList.Sort(Comparer);
			var imageList = new List<ImageViewModel>();
			foreach (var info in infoList)
			{
				var img = new ImageViewModel(Resources!.Self, info.Hash);
				img.Update(force);
				imageList.Add(img);
			}
			Images = imageList;
		}

		public static int Comparer(ImageInfo x, ImageInfo y)
		{
			if (x.LastUpdated < y.LastUpdated) return 1;
			else if (x.LastUpdated > y.LastUpdated) return -1;
			else return 0;
		}
	}
}