using ClientWPF.Models.Components;
using ClientWPF.ViewModels.Components;
using Hosta.API.Data;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class InfoViewModel : ObservableObject
	{
		public ICommand Refresh { get; private set; }

		public ICommand StartEditing { get; private set; }

		private ProfileViewModel _profile = new ProfileViewModel(Resources!.Self);

		public ProfileViewModel Profile
		{
			get => _profile;
			set
			{
				_profile = value;
				NotifyPropertyChanged(nameof(Profile));
			}
		}

		public InfoViewModel(Action<Profile> OnEdit)
		{
			Refresh = new RelayCommand((object? _) => Update());
			StartEditing = new RelayCommand((object? _) => OnEdit(Profile.Profile));
			Update();
		}

		public async void Update()
		{
			await Profile.Refresh();
		}
	}
}