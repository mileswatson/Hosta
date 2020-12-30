using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class InfoViewModel
	{
		public ICommand StartEditing { get; private set; }

		public InfoViewModel(Action OnEdit)
		{
			StartEditing = new RelayCommand((object _) => OnEdit());
		}
	}
}