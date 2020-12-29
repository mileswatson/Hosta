using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class ProfileInfoViewModel
	{
		private ProfileTabViewModel parent;

		public ICommand EditButtonClicked { get; private set; }

		public ProfileInfoViewModel(ProfileTabViewModel parent)
		{
			this.parent = parent;
			EditButtonClicked = new RelayCommand((object _) => { parent.Switch(); });
		}
	}
}