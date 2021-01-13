using ClientWPF.Models.Data;
using System.Windows.Media.Imaging;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.Components
{
	public class ProfileViewModel : ObservableObject
	{
		public string Name { get => Profile.Name; }

		public string ID { get => Profile.ID; }

		public string Tagline { get => Profile.Tagline; }

		public string Bio { get => Profile.Bio; }

		private ImageViewModel _image = new ImageViewModel();

		public ImageViewModel Image
		{
			get => _image;
			private set
			{
				_image = value;
				NotifyPropertyChanged(nameof(Image));
			}
		}

		private readonly string id;

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
			}
		}

		public ProfileViewModel()
		{
			id = "";
		}

		public ProfileViewModel(string id)
		{
			this.id = id;
		}

		public override async void Update(bool force)
		{
			var newProfile = await Resources!.GetProfile(id, force);
			if (Profile != newProfile) Profile = newProfile;
			if (Profile.AvatarHash != Image.Hash) Image = new ImageViewModel(id, Profile.AvatarHash);
			Image.Update(force);
		}
	}
}