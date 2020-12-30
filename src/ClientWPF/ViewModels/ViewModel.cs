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
			VM = new StartupViewModel(this);
		}

		public void Connected()
		{
			VM = new ConnectedViewModel();
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