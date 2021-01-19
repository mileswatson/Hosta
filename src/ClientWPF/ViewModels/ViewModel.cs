using ClientWPF.Models;
using Hosta.API;
using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels
{
	public class ViewModel : ObservableObject
	{
		private readonly StartupViewModel startup;

		private ObservableObject _vm;

		public ObservableObject VM
		{
			get
			{
				return _vm;
			}
			set
			{
				_vm = value;
				NotifyPropertyChanged(nameof(VM));
				_vm.Update(false);
			}
		}

		public ViewModel()
		{
			_vm = startup = new StartupViewModel(OnConnect, OnQuit);
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

			APITranslatorClient nodeConnection;
			try
			{
				nodeConnection = await APITranslatorClient.CreateAndConnect(new APITranslatorClient.ConnectionArgs
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

		public override Task UpdateAsync(bool force)
		{
			VM.Update(force);
			return Task.CompletedTask;
		}
	}
}