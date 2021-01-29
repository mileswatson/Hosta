using ClientWPF.Views.HomeTab;
using Hosta.API;
using Hosta.API.Friend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static ClientWPF.ApplicationEnvironment;
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

		private Action<FriendViewModel> clicked;

		public PeopleViewModel(Action<string> userClick)
		{
			clicked = (FriendViewModel friend) =>
			{
				userClick(friend.ID);
			};
			Self = new FriendViewModel(new FriendInfo
			{
				ID = Resources!.Self,
				Name = "You",
				IsFavorite = true,
			}, new(), clicked);
		}

		private void MakeFavorite(FriendViewModel? friend) => SetFavoriteStatus(friend, true);

		private void Unfavorite(FriendViewModel? friend) => SetFavoriteStatus(friend, false);

		private async void SetFavoriteStatus(FriendViewModel? friend, bool isFavorite)
		{
			if (friend is null) throw new NullReferenceException();
			try
			{
				await Resources!.SetFriend(friend.ID, friend.Name, isFavorite);
				Env.Alert("Changed favorite status.");
			}
			catch (APIException e)
			{
				Env.Alert($"Could not change favorite status! {e.Message}");
			}
			catch
			{
				Env.Alert("Could not change favorite status!");
			}
			Update(false);
		}

		private void ManuallyConnect(FriendViewModel? friend)
		{
			if (friend is null) throw new NullReferenceException();

			var model = new ManuallyConnectWindow(async (window) =>
			{
				IPAddress address;
				try
				{
					address = IPAddress.Parse(window.IP.Text);
				}
				catch
				{
					Env.Alert("Invalid IP format!");
					return;
				}

				int port;
				try
				{
					port = int.Parse(window.Port.Text);
				}
				catch
				{
					Env.Alert("Invalid port format!");
					return;
				}

				try
				{
					await Resources!.AddAddress(friend.ID, address, port);
					window.Close();
					friend.Click.Execute(friend);
					Update(true);
				}
				catch (APIException e)
				{
					Env.Alert($"Could not manually connect! {e.Message}");
				}
				catch
				{
					Env.Alert($"Could not manually connect!");
				}
			});
			model.ShowDialog();
		}

		public async void RemoveFriend(FriendViewModel? friend)
		{
			if (friend is null) throw new NullReferenceException();
			try
			{
				await Resources!.RemoveFriend(friend.ID);
				Env.Alert("Removed friend.");
			}
			catch (APIException e)
			{
				Env.Alert($"Could not remove friend! {e.Message}");
			}
			catch
			{
				Env.Alert("Could not remove friend!");
			}
			Update(false);
		}

		public override async Task UpdateAsync(bool force)
		{
			Self.Update(force);

			var response = await Resources!.GetFriendList();

			response.Sort(Compare);

			var friendMenuItems = new List<ContextMenuItem<FriendViewModel>>
			{
				new("Make favorite", MakeFavorite),
				new("Manually connect", ManuallyConnect),
				new("Remove friend", RemoveFriend)
			};

			var friends = response.Select(info => new FriendViewModel(
				info,
				friendMenuItems,
				clicked
			)).ToList();

			var favorites = friends.Where(friend =>
			{
				if (!friend.IsFavorite) return false;
				friend.MenuItems = new() { new("Remove favorite", Unfavorite) };
				return true;
			}).ToList();

			Favorites = favorites;
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