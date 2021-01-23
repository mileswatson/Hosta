using ClientWPF.ViewModels.Components;
using Hosta.API.Friend;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientWPF.ViewModels.HomeTab
{
	public class FriendViewModel : ObservableObject
	{
		public ProfileViewModel Profile { get; init; }

		public string ID { get; init; }

		public string Name { get; init; }

		public bool IsFavorite { get; init; }

		private List<ContextMenuItem<FriendViewModel>> _menuItems;

		public List<ContextMenuItem<FriendViewModel>> MenuItems
		{
			get => _menuItems;
			set
			{
				_menuItems = value;
				NotifyPropertyChanged(nameof(MenuItems));
			}
		}

		public FriendViewModel(FriendInfo info, List<ContextMenuItem<FriendViewModel>> menuItems)
		{
			ID = info.ID;
			Name = info.Name;
			IsFavorite = info.IsFavorite;
			_menuItems = menuItems;
			Profile = new ProfileViewModel(info.ID);
		}

		public override Task UpdateAsync(bool force)
		{
			Profile.Update(force);
			return Task.CompletedTask;
		}
	}
}