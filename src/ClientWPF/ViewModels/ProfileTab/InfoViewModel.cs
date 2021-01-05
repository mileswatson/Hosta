using System;
using System.Windows.Input;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class InfoViewModel
	{
		public ICommand StartEditing { get; private set; }

		public InfoViewModel(Action OnEdit)
		{
			StartEditing = new RelayCommand((object? _) => OnEdit());
		}
	}
}