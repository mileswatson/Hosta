using System;
using System.Windows.Input;

namespace ClientWPF.ViewModels.SettingsTab
{
	public class ViewModel
	{
		public ICommand Disconnect { get; private set; }

		public ViewModel(Action OnDisconnect)
		{
			Disconnect = new RelayCommand((object? _) => OnDisconnect());
		}
	}
}