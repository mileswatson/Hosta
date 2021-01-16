using ClientWPF.Models.Data;
using System.Windows.Media.Imaging;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.Components
{
	internal class PostViewModel : ObservableObject
	{
		public string Name { get => Profile.Name; }

		public string ID { get => Profile.ID; }

		public BitmapImage Avatar { get => DefaultImage; }

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
				NotifyPropertyChanged(nameof(Avatar));
			}
		}

		public string Content { get => Post.Content; }

		public string LastUpdated
		{
			get => Post.TimePosted.ToShortDateString();
		}

		private Post _post = new();

		private Post Post
		{
			get => _post;
			set
			{
				_post = value;
				NotifyPropertyChanged(nameof(Post));
				NotifyPropertyChanged(nameof(Content));
				NotifyPropertyChanged(nameof(LastUpdated));
			}
		}

		public override void Update(bool force)
		{
			UpdateProfile(force);
			UpdatePost(force);
		}

		private async void UpdateProfile(bool force)
		{
			var newProfile = await Resources!.GetProfile(ID, force);
			if (Profile != newProfile) Profile = newProfile;
		}

		private void UpdatePost(bool force)
		{
		}
	}
}