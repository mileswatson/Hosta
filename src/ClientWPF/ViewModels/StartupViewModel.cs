using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class StartupViewModel : INotifyPropertyChanged
	{
		private ViewModel parent;

		public ICommand ConnectButtonClicked { get; private set; }

		public StartupViewModel(ViewModel parent)
		{
			this.parent = parent;
			ConnectButtonClicked = new RelayCommand((object _) => { parent.Connected(); });
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