using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientWPF.ViewModels
{
	public class ProfileTabViewModel : INotifyPropertyChanged
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

		private ProfileInfoViewModel profileInfo;

		private ProfileEditViewModel profileEdit;

		public ProfileTabViewModel()
		{
			profileInfo = new(this);
			profileEdit = new(this);
			VM = profileInfo;
		}

		/// <summary>
		/// Switches between info and edit views.
		/// </summary>
		public void Switch()
		{
			if (VM == profileInfo) VM = profileEdit;
			else if (VM == profileEdit) VM = profileInfo;
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