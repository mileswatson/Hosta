using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using static ClientWPF.ApplicationEnvironment;

namespace ClientWPF.ViewModels
{
	public class ViewModel : INotifyPropertyChanged
	{
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
			_vm = startup = new StartupViewModel(OnConnect, OnQuit);
			Transcoder.BytesFromHex("A4");
		}

		public void OnQuit() => Environment.Exit(0);

		public async void OnConnect(string folder, IPEndPoint endpoint)
		{
			var file = Path.Combine(folder, "client.identity");
			PrivateIdentity identity;
			if (!CheckFile(file))
			{
				if (!Confirm($"No identity was found at {file}. Do you want to create a new one?"))
					return;
				identity = PrivateIdentity.Create();
				var hex = Transcoder.HexFromBytes(PrivateIdentity.Export(identity));
				await WriteFile(file, hex);
				Alert($"Identity {identity.ID} has been created and saved.");
			}
			else
			{
				var hex = Transcoder.BytesFromHex(await ReadFile(file));
				identity = PrivateIdentity.Import(hex);
			}
			Alert($"Loaded {identity.ID} from {folder}, will connect to {endpoint}");
			VM = new ConnectedViewModel(OnDisconnect);
		}

		public void OnDisconnect() => VM = startup;

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