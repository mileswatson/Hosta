using ClientWPF.Models.Components;
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

		public InfoViewModel(Action<Profile> OnEdit)
		{
			Refresh = new RelayCommand((object? _) => Update(true));
			Profile = new ProfileViewModel(Resources!.Self);
			StartEditing = new RelayCommand((object? _) => OnEdit(Profile.Profile));
		}

		public override void Update(bool force)
		{
			Profile.Update(force);
		}
	}
}