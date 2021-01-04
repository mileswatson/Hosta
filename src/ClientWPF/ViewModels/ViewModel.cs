using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.ComponentModel;
using System.IO;
using static ClientWPF.ApplicationEnvironment;

namespace ClientWPF.ViewModels
{
	public class ViewModel : INotifyPropertyChanged
	{
		private readonly ConnectViewModel connect;
		private readonly StartupViewModel startup;

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

		public ViewModel()
		{
			connect = new ConnectViewModel(OnConnect, OnChangeProfile);
			_vm = startup = new StartupViewModel(OnFolderSelected, OnQuit);
			Transcoder.BytesFromHex("A4");
		}

		public void OnQuit() => Environment.Exit(0);

		public async void OnFolderSelected(string folder)
		{
			if (!CheckDirectory(folder))
			{
				Alert("Folder does not exist!");
				return;
			}
			var file = Path.Combine(folder, "client.identity");
			if (!CheckFile(file))
			{
				if (!Confirm($"No identity was found at {file}. Do you want to create a new one?"))
					return;
				var privateIdentity = PrivateIdentity.Create();
				var hex = Transcoder.HexFromBytes(PrivateIdentity.Export(privateIdentity));
				await WriteFile(file, hex);
				Alert($"Identity {privateIdentity.ID} has been created and saved.");
			}
			else
			{
				var hex = Transcoder.BytesFromHex(await ReadFile(file));
				var privateIdentity = PrivateIdentity.Import(hex);
				Alert($"Loaded identity {privateIdentity.ID}.");
			}
			VM = connect;
		}

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