using ClientWPF.Models.Data;
using ClientWPF.ViewModels.Components;
using Hosta.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class InfoViewModel : ObservableObject
	{
		public ICommand Refresh { get; init; }

		public ICommand StartEditing { get; init; }

		public ProfileViewModel Profile { get; init; }

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

		public InfoViewModel(Action<Profile> OnEdit)
		{
			Refresh = new RelayCommand((object? _) => Update(true));
			Profile = new ProfileViewModel(Resources!.Self);
			StartEditing = new RelayCommand((object? _) => OnEdit(Profile.Profile));

			var menuItems = new List<ContextMenuItem<PostViewModel>>
			{
				new("Remove", async (PostViewModel? p) =>
				{
					if (p is null) return;
					try
					{
						await Resources!.RemovePost(p.ID);
						Env.Alert("Post removed.");
						Update(false);
					}
					catch (APIException e)
					{
						Env.Alert($"Could not remove post! {e.Message}");
					}
					catch
					{
						Env.Alert("Could not remove post!");
					}
				})
			};

			_feed = new PostFeedViewModel(Resources!.Self, menuItems);
		}

		public override Task UpdateAsync(bool force)
		{
			Profile.Update(force);
			Feed.Update(force);
			return Task.CompletedTask;
		}
	}
}