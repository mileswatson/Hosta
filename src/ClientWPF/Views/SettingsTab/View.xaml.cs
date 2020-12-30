using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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