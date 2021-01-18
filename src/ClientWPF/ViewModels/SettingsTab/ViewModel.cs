using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels.SettingsTab
{
	public class ViewModel : ObservableObject
	{
		public ICommand Disconnect { get; private set; }

		public ViewModel(Action OnDisconnect)
		{
			Disconnect = new RelayCommand((object? _) => OnDisconnect());
		}

		public override Task UpdateAsync(bool force) => Task.CompletedTask;
	}
}