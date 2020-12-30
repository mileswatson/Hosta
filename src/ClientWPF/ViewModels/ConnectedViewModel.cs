using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class ConnectedViewModel : INotifyPropertyChanged
	{
		private ViewModels.HomeTab.ViewModel homeTab;
		private ViewModels.PostTab.ViewModel postTab;
		private ViewModels.ProfileTab.ViewModel profileTab;
		private ViewModels.SettingsTab.ViewModel settingsTab;

		private object _vm;

		public object VM
		{
			get
			{
				return _vm;
			}
			set
			{
				_vm = value;
				NotifyPropertyChanged(nameof(VM));
			}
		}

		public ICommand HomeTab { get; private set; }
		public ICommand PostTab { get; private set; }
		public ICommand ProfileTab { get; private set; }
		public ICommand SettingsTab { get; private set; }

		public ConnectedViewModel(Action OnDisconnect)
		{
			homeTab = new();
			postTab = new();
			profileTab = new();
			settingsTab = new(() => OnDisconnect());

			VM = homeTab;
			HomeTab = new RelayCommand((object _) => { VM = homeTab; });
			PostTab = new RelayCommand((object _) => { VM = postTab; });
			ProfileTab = new RelayCommand((object _) => { VM = profileTab; });
			SettingsTab = new RelayCommand((object _) => { VM = settingsTab; });
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}