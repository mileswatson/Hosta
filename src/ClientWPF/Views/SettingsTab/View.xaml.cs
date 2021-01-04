using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views.SettingsTab
{
	/// <summary>
	/// Interaction logic for SettingsTabView.xaml
	/// </summary>
	public partial class View : UserControl
	{
		public View()
		{
			InitializeComponent();
		}

		public void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to disconnect?", "Alert", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes)
			{
				var vm = (ViewModels.SettingsTab.ViewModel)DataContext;
				if (vm.Disconnect.CanExecute(null)) vm.Disconnect.Execute(null);
			}
		}
	}
}