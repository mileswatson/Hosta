using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class StartupViewModel : INotifyPropertyChanged
	{
		public ICommand FolderSelected { get; private set; }
		public ICommand Quit { get; private set; }

		private string folder = "folder path";

		public string Folder
		{
			get { return folder; }
			set
			{
				folder = value;
				NotifyPropertyChanged(nameof(Folder));
			}
		}

		public StartupViewModel(Action<string> OnFolderSelected, Action OnQuit)
		{
			FolderSelected = new RelayCommand((object? _) => OnFolderSelected(Folder));
			Quit = new RelayCommand((object? _) => OnQuit());
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}