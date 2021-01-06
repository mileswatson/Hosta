using Hosta.API.Data;
using System;
using System.ComponentModel;
using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class InfoViewModel : INotifyPropertyChanged
	{
		public ICommand Refresh { get; private set; }

		public ICommand StartEditing { get; private set; }

		private GetProfileResponse profile = new GetProfileResponse();

		public GetProfileResponse Profile
		{
			get => profile;
			set
			{
				profile = value;
				NotifyPropertyChanged(nameof(Profile));
			}
		}

		public InfoViewModel(Action OnEdit)
		{
			Refresh = new RelayCommand((object? _) => Update());
			StartEditing = new RelayCommand((object? _) => OnEdit());
		}

		public async void Update()
		{
			try
			{
				Profile = await Resources!.GetProfile(Resources!.Self);
			}
			catch
			{
				Profile = new();
			}
		}

		//// Implement INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}