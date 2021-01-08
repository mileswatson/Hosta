using static ClientWPF.Models.ResourceManager;
using ClientWPF.Models.Components;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClientWPF.ViewModels.Components
{
	public class ProfileViewModel : INotifyPropertyChanged
	{
		private Profile _profile = new();

		private Profile Profile
		{
			get => _profile;
			set
			{
				_profile = value;
				NotifyPropertyChanged(nameof(Profile));
				NotifyPropertyChanged(nameof(Name));
				NotifyPropertyChanged(nameof(ID));
				NotifyPropertyChanged(nameof(Tagline));
				NotifyPropertyChanged(nameof(Bio));
				NotifyPropertyChanged(nameof(AvatarImage));
				changed = true;
			}
		}

		public string Name { get => Profile.DisplayName; }

		public string ID { get => Profile.ID.Length < 12 ? Profile.ID : Profile.ID.Substring(0, 12) + "..."; }

		public string Tagline { get => Profile.Tagline; }

		public string Bio { get => Profile.Bio; }

		public string LastUpdated { get => Profile.LastUpdated; }

		private string id;

		private bool changed = true;

		private BitmapImage _avatarImage = new BitmapImage();

		public BitmapImage AvatarImage
		{
			get
			{
				if (!changed) return _avatarImage;
				var image = new BitmapImage();
				try
				{
					using (var stream = new MemoryStream(Profile.Avatar, 0, Profile.Avatar.Length))
					{
						image.BeginInit();
						image.DecodePixelWidth = 160;
						image.CacheOption = BitmapCacheOption.None;
						image.StreamSource = stream;
						image.EndInit();
					}
				}
				catch
				{
					image = new BitmapImage();
					image.BeginInit();
					image.DecodePixelWidth = 160;
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.UriSource = new Uri("Assets/Images/default-avatar.png", UriKind.Relative);
					image.EndInit();
				}
				_avatarImage = image;
				changed = false;
				return image;
			}
		}

		public ProfileViewModel(string id)
		{
			this.id = id;
		}

		public async Task Refresh()
		{
			var newProfile = await Resources!.GetProfile(id);
			if (Profile != newProfile) Profile = newProfile;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}