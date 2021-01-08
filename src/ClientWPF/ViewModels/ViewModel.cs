using ClientWPF.Models;
using Hosta.API;
using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

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
			if (!Env.CheckFile(file))
			{
				if (!Env.Confirm($"No identity was found at {file}. Do you want to create a new one?"))
					return;
				identity = PrivateIdentity.Create();
				var hex = Transcoder.HexFromBytes(PrivateIdentity.Export(identity));
				await Env.WriteFile(file, hex);
			}
			else
			{
				var hex = Transcoder.BytesFromHex(await Env.ReadFile(file));
				identity = PrivateIdentity.Import(hex);
			}

			RemoteAPIGateway nodeConnection;
			try
			{
				nodeConnection = await RemoteAPIGateway.CreateAndConnect(new RemoteAPIGateway.ConnectionArgs
				{
					Self = identity,
					ServerID = identity.ID,
					Address = endpoint.Address,
					Port = endpoint.Port
				});
			}
			catch
			{
				Env.Alert("Unable to connect to your node!");
				return;
			}

			Resources = new ResourceManager(identity, nodeConnection, OnDisconnect);
			VM = new ConnectedViewModel(OnDisconnect);
		}

		public void OnDisconnect()
		{
			VM = startup;
			Resources?.Dispose();
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