using System;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class ConnectedViewModel : ObservableObject
	{
		private readonly HomeTab.ViewModel homeTab;
		private readonly PostTab.ViewModel postTab;
		private readonly ImagesTab.ViewModel imagesTab;
		private readonly ProfileTab.ViewModel profileTab;
		private readonly SettingsTab.ViewModel settingsTab;

		private ObservableObject _vm;

		public ObservableObject VM
		{
			get
			{
				return _vm;
			}
			private set
			{
				_vm = value;
				NotifyPropertyChanged(nameof(VM));
				_vm.Update(false);
			}
		}

		public ICommand HomeTab { get; private set; }
		public ICommand PostTab { get; private set; }
		public ICommand ImagesTab { get; private set; }
		public ICommand ProfileTab { get; private set; }
		public ICommand SettingsTab { get; private set; }

		public ConnectedViewModel(Action OnDisconnect)
		{
			homeTab = new();
			postTab = new();
			imagesTab = new();
			profileTab = new();
			settingsTab = new(() => OnDisconnect());

			_vm = homeTab;
			HomeTab = new RelayCommand((object? _) => { VM = homeTab; });
			PostTab = new RelayCommand((object? _) => { VM = postTab; });
			ImagesTab = new RelayCommand((object? _) => { VM = imagesTab; });
			ProfileTab = new RelayCommand((object? _) => { VM = profileTab; });
			SettingsTab = new RelayCommand((object? _) => { VM = settingsTab; });
		}

		public override void Update(bool force)
		{
			VM.Update(force);
		}
	}
}