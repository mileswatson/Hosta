using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class ConnectViewModel : INotifyPropertyChanged
	{
		public ICommand Connect { get; private set; }
		public ICommand ChangeProfile { get; private set; }

		public ConnectViewModel(Action OnConnect, Action OnChangeProfile)
		{
			Connect = new RelayCommand((object? _) => OnConnect());
			ChangeProfile = new RelayCommand((object? _) => OnChangeProfile());
		}

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