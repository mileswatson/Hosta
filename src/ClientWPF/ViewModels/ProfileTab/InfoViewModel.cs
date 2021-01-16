using ClientWPF.Models.Data;
using ClientWPF.ViewModels.Components;
using System;
using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class InfoViewModel : ObservableObject
	{
		public ICommand Refresh { get; init; }

		public ICommand StartEditing { get; init; }

		public ProfileViewModel Profile { get; init; }

		private PostFeedViewModel _feed = new PostFeedViewModel(Resources!.Self);

		public PostFeedViewModel Feed
		{
			get => _feed;
			set
			{
				_feed = value;
				NotifyPropertyChanged(nameof(Feed));
			}
		}

		public InfoViewModel(Action<Profile> OnEdit)
		{
			Refresh = new RelayCommand((object? _) => Update(true));
			Profile = new ProfileViewModel(Resources!.Self);
			StartEditing = new RelayCommand((object? _) => OnEdit(Profile.Profile));
		}

		public override void Update(bool force)
		{
			Profile.Update(force);
			Feed.Update(force);
		}
	}
}