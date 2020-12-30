using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels.SettingsTab
{
	public class ViewModel
	{
		public ICommand Disconnect { get; private set; }

		public ViewModel(Action OnDisconnect)
		{
			Disconnect = new RelayCommand((object _) => OnDisconnect());
		}
	}
}