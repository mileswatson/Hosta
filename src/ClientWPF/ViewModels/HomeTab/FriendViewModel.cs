using ClientWPF.ViewModels.Components;
using Hosta.API.Friend;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

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

		public ICommand Click { get; init; }

		public FriendViewModel(FriendInfo info, List<ContextMenuItem<FriendViewModel>> menuItems, Action<FriendViewModel> click)
		{
			ID = info.ID;
			Name = info.Name;
			IsFavorite = info.IsFavorite;
			_menuItems = menuItems;
			Profile = new ProfileViewModel(info.ID);
			Click = new RelayCommand((object? o) =>
			{
				var friend = o as FriendViewModel ?? throw new NullReferenceException();
				click(friend);
			});
		}

		public override Task UpdateAsync(bool force)
		{
			Profile.Update(force);
			return Task.CompletedTask;
		}
	}
}