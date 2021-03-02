using ClientWPF.ViewModels.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.HomeTab
{
	public class ViewModel : ObservableObject
	{
		private ProfileViewModel? _profile;

		public ProfileViewModel? Profile
		{
			get => _profile;
			set
			{
				_profile = value;
				NotifyPropertyChanged(nameof(Profile));
			}
		}

		private PostFeedViewModel _feed;

		public PostFeedViewModel Feed
		{
			get => _feed;
			set
			{
				_feed = value;
				NotifyPropertyChanged(nameof(Feed));
			}
		}

		public PeopleViewModel Friends { get; init; }

		public ViewModel()
		{
			_feed = new PostFeedViewModel(Resources.Self, new());
			Friends = new PeopleViewModel((string id) =>
			{
				Feed = new(id, new());
				Profile = new(id);
				Update(false);
			});
			_profile = new ProfileViewModel(Resources.Self);
		}

		public override Task UpdateAsync(bool force)
		{
			Feed.Update(force);
			Friends.Update(force);
			Profile?.Update(force);
			return Task.CompletedTask;
		}
	}
}