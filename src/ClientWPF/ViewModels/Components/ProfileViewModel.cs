using static ClientWPF.Models.ResourceManager;
using ClientWPF.Models.Components;
using System.Windows.Media.Imaging;

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
				NotifyPropertyChanged(nameof(Avatar));
			}
		}

		public string Name { get => Profile.DisplayName; }

		public string ID { get => Profile.ID; }

		public string Tagline { get => Profile.Tagline; }

		public string Bio { get => Profile.Bio; }

		public string LastUpdated { get => Profile.LastUpdated; }

		public BitmapImage Avatar { get => Profile.Avatar; }

		private readonly string id;

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
		}
	}
}