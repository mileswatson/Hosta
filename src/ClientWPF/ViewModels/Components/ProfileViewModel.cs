using static ClientWPF.Models.ResourceManager;
using ClientWPF.Models.Components;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace ClientWPF.ViewModels.Components
{
	public class ProfileViewModel : ObservableObject
	{
		private Profile _profile = new();

		public Profile Profile
		{
			get => _profile;
			private set
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

		public string ID { get => Profile.ID; }

		public string Tagline { get => Profile.Tagline; }

		public string Bio { get => Profile.Bio; }

		public string LastUpdated { get => Profile.LastUpdated; }

		private string id = "";

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

		public ProfileViewModel()
		{
		}

		public ProfileViewModel(string id)
		{
			this.id = id;
		}

		public override async void Update(bool force)
		{
			var newProfile = await Resources!.GetProfile(id, force);
			if (Profile != newProfile) Profile = newProfile;
		}
	}
}