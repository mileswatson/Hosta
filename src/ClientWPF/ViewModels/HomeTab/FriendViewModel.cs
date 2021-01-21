using ClientWPF.ViewModels.Components;
using Hosta.API.Friend;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientWPF.ViewModels.HomeTab
{
	public class FriendViewModel : ObservableObject
	{
		public ProfileViewModel Profile { get; init; }

		public string Name { get; init; }

		public bool IsFavorite { get; init; }

		public List<ContextMenuItem<FriendViewModel>> MenuItems { get; init; }

		public FriendViewModel(FriendInfo info, List<ContextMenuItem<FriendViewModel>> menuItems)
		{
			Profile = new ProfileViewModel(info.ID);
			Name = info.Name;
			IsFavorite = info.IsFavorite;
			MenuItems = menuItems;
		}

		public override Task UpdateAsync(bool force)
		{
			Profile.Update(force);
			return Task.CompletedTask;
		}
	}
}