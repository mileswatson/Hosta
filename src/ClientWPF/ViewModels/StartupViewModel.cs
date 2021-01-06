using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Input;
using static ClientWPF.ApplicationEnvironment;

namespace ClientWPF.ViewModels
{
	public class StartupViewModel : INotifyPropertyChanged
	{
		public ICommand Continue { get; private set; }
		public ICommand Quit { get; private set; }

		private string folder = "folder path";

		public string Folder
		{
			get => folder;
			set
			{
				folder = value;
				NotifyPropertyChanged(nameof(Folder));
			}
		}

		private string ip = "127.0.0.1";

		public string IP
		{
			get => ip;
			set
			{
				ip = value;
				NotifyPropertyChanged(nameof(IP));
			}
		}

		private string port = "12000";

		public string Port
		{
			get => port;
			set
			{
				port = value;
				NotifyPropertyChanged(nameof(Port));
			}
		}

		public StartupViewModel(Action<string, IPEndPoint> OnConnect, Action OnQuit)
		{
			Continue = new RelayCommand((object? _) =>
			{
				string folder = Folder;

				// Ensure that directory exists
				if (!Env.CheckDirectory(folder))
				{
					Env.Alert("Folder does not exist!");
					return;
				}

				// Ensure that the IP address is valid
				IPAddress address;
				try
				{
					address = IPAddress.Parse(IP.Trim());
				}
				catch
				{
					Env.Alert("The IP field was in an invalid format!");
					return;
				}

				// Ensure that the port is an integer, and in the valid range
				int port;
				try
				{
					port = int.Parse(Port.Trim());
					if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
						throw new Exception();
				}
				catch
				{
					Env.Alert("The port field was in an invalid format!");
					return;
				}

				OnConnect(folder, new IPEndPoint(address, port));
			});
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