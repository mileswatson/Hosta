using ClientWPF.ViewModels.Components;
using Hosta.API;
using Hosta.API.Image;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.ImagesTab
{
	public class ViewModel : ObservableObject
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

		public ICommand Refresh { get; set; }

		public ICommand Remove { get; set; }

		public ViewModel()
		{
			Refresh = new RelayCommand((object? _) => Update(true));
			Remove = new RelayCommand((object? o) => RemoveImage((string)(o ?? "")));
		}

		public async void Upload(string filename)
		{
			byte[] data = await Env.ReadFileRaw(filename);
			try
			{
				ImageFromBytes(data);
			}
			catch
			{
				Env.Alert("Could not parse image!");
			}
			try
			{
				await Resources.AddImage(data);
				Update(true);
			}
			catch (APIException e)
			{
				Env.Alert($"Could not upload image! {e.Message}");
			}
			catch
			{
				Env.Alert("Could not upload image!");
			}
		}

		public async void RemoveImage(string hash)
		{
			try
			{
				await Resources.RemoveImage(hash);
				Update(true);
			}
			catch (APIException e)
			{
				Env.Alert($"Could not delete image! {e.Message}");
			}
			catch
			{
				Env.Alert("Could not delete image!");
			}
		}

		public override async Task UpdateAsync(bool force)
		{
			var infoList = await Resources.GetImageList();
			infoList.Sort(Comparer);
			var imageList = new List<ImageViewModel>();
			foreach (var info in infoList)
			{
				var img = new ImageViewModel(Resources.Self, info.Hash);
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