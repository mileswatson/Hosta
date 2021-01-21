using ClientWPF.ViewModels.Components;
using Hosta.API.Friend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.HomeTab
{
	public class PeopleViewModel : ObservableObject
	{
		public FriendViewModel Self { get; init; }

		private List<FriendViewModel> _bestfriends = new();

		public List<FriendViewModel> Favorites
		{
			get => _bestfriends;
			private set
			{
				_bestfriends = value;
				NotifyPropertyChanged(nameof(Favorites));
			}
		}

		private List<FriendViewModel> _friends = new();

		public List<FriendViewModel> Friends
		{
			get => _friends;
			private set
			{
				_friends = value;
				NotifyPropertyChanged(nameof(Friends));
			}
		}

		public PeopleViewModel()
		{
			Self = new FriendViewModel(new FriendInfo
			{
				ID = Resources!.Self,
				Name = "You",
				IsFavorite = true
			}, new());
		}

		public override async Task UpdateAsync(bool force)
		{
			Self.Update(force);

			var response = await Resources!.GetFriendList();

			response.Sort(Compare);

			var friends = response.Select(info => new FriendViewModel(
				info,
				new()
			)).ToList();

			var favourites = friends.Where(friend => friend.IsFavorite).ToList();

			Favorites = favourites;
			Friends = friends;

			foreach (var friend in Friends)
			{
				friend.Update(force);
			}
			foreach (var friend in Favorites)
			{
				friend.Update(force);
			}
		}

		private int Compare(FriendInfo x, FriendInfo y)
		{
			return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
		}
	}
}