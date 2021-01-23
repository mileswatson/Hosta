using System.Windows;

namespace ClientWPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			ApplicationEnvironment.Env = new WindowEnvironment(e.Args.Length == 1 ? e.Args[0] : "");
		}

		private class WindowEnvironment : ApplicationEnvironment
		{
			public override void Alert(string message)
			{
				MessageBox.Show(message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			public override bool Confirm(string message)
			{
				var result = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
				return result == MessageBoxResult.Yes;
			}

			public WindowEnvironment(string defaultFolder) : base()
			{
				DefaultFolder = defaultFolder;
			}
		}
	}
}