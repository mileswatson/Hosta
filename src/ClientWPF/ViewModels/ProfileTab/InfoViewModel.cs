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
		private ViewModel parent;

		public ICommand EditButtonClicked { get; private set; }

		public InfoViewModel(ViewModel parent)
		{
			this.parent = parent;
			EditButtonClicked = new RelayCommand((object _) => { parent.Switch(); });
		}
	}
}