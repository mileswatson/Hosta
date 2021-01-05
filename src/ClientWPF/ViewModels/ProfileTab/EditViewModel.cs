using System;
using System.Windows.Input;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class EditViewModel
	{
		public ICommand CancelEditing { get; private set; }

		public EditViewModel(Action OnCancel)
		{
			CancelEditing = new RelayCommand((object? _) => OnCancel());
		}
	}
}