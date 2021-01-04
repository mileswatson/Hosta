using System;
using System.ComponentModel;

namespace ClientWPF.ViewModels
{
	public class ViewModel : INotifyPropertyChanged
	{
		private object _vm;

		private readonly ConnectViewModel connect;
		private readonly StartupViewModel startup;

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

		public ViewModel()
		{
			connect = new ConnectViewModel(OnConnect, OnChangeProfile);
			_vm = startup = new StartupViewModel(OnContinue, OnQuit);
		}

		public void OnQuit() => Environment.Exit(0);

		public void OnContinue() => VM = connect;

		public void OnChangeProfile() => VM = startup;

		public void OnConnect() => VM = new ConnectedViewModel(OnDisconnect);

		public void OnDisconnect() => VM = connect;

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