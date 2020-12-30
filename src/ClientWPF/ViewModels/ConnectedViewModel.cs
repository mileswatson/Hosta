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
		private ViewModels.HomeTab.ViewModel homeTab = new();
		private ViewModels.PostTab.ViewModel postTab = new();
		private ViewModels.ProfileTab.ViewModel profileTab = new();
		private ViewModels.SettingsTab.ViewModel settingsTab = new();

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

		public ICommand HomeButtonClicked { get; private set; }
		public ICommand PostButtonClicked { get; private set; }
		public ICommand ProfileButtonClicked { get; private set; }
		public ICommand SettingsButtonClicked { get; private set; }

		public ConnectedViewModel()
		{
			VM = homeTab;
			HomeButtonClicked = new RelayCommand((object _) => { VM = homeTab; });
			PostButtonClicked = new RelayCommand((object _) => { VM = postTab; });
			ProfileButtonClicked = new RelayCommand((object _) => { VM = profileTab; });
			SettingsButtonClicked = new RelayCommand((object _) => { VM = settingsTab; });
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