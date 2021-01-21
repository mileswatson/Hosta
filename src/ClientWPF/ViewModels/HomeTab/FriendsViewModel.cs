using ClientWPF.ViewModels.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.HomeTab
{
	public class FriendsViewModel : ObservableObject
	{
		private List<ProfileViewModel> _bestfriends = new();

		public List<ProfileViewModel> BestFriends
		{
			get => _bestfriends;
			private set
			{
				_bestfriends = value;
				NotifyPropertyChanged(nameof(BestFriends));
			}
		}

		private List<ProfileViewModel> _friends = new();

		public List<ProfileViewModel> Friends
		{
			get => _friends;
			private set
			{
				_friends = value;
				NotifyPropertyChanged(nameof(Friends));
			}
		}

		public override Task UpdateAsync(bool force)
		{
			BestFriends = new()
			{
				new ProfileViewModel(Resources!.Self),
				new ProfileViewModel(Resources!.Self)
			};
			Friends = new()
			{
				new ProfileViewModel(Resources!.Self),
				new ProfileViewModel(Resources!.Self),
				new ProfileViewModel(Resources!.Self)
			};
			foreach (var friend in Friends)
			{
				friend.Update(force);
			}
			foreach (var friend in BestFriends)
			{
				friend.Update(force);
			}
			return Task.CompletedTask;
		}
	}
}