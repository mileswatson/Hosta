using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientWPF.ViewModels
{
	public class ViewModel : INotifyPropertyChanged
	{
		private object _vm;

		private StartupViewModel startup;

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

		public ViewModel()
		{
			VM = startup = new StartupViewModel(OnConnect);
		}

		public void OnConnect()
		{
			VM = new ConnectedViewModel(OnDisconnect);
		}

		public void OnDisconnect()
		{
			VM = startup;
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