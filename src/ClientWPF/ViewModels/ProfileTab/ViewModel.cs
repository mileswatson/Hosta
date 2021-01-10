using ClientWPF.Models.Components;
using System.ComponentModel;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class ViewModel : ObservableObject
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

		private readonly InfoViewModel profileInfo;

		public ViewModel()
		{
			profileInfo = new(StartEditing);
			_vm = profileInfo;
		}

		public void StartEditing(Profile profile)
		{
			VM = new EditViewModel(StopEditing, profile);
		}

		public void StopEditing()
		{
			VM = profileInfo;
		}
	}
}