using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class ProfileEditViewModel
	{
		private ProfileTabViewModel parent;

		public ICommand CancelButtonClicked { get; private set; }

		public ProfileEditViewModel(ProfileTabViewModel parent)
		{
			this.parent = parent;
			CancelButtonClicked = new RelayCommand((object _) => { parent.Switch(); });
		}
	}
}