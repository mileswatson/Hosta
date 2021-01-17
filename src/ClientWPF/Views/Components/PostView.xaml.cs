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

namespace ClientWPF.Views.Components
{
	/// <summary>
	/// Interaction logic for PostView.xaml
	/// </summary>
	public partial class PostView : UserControl
	{
		public PostView()
		{
			InitializeComponent();
		}

		public void DropdownButton_Clicked(object sender, RoutedEventArgs e)
		{
			DropdownMenu.IsEnabled = true;
			DropdownMenu.IsOpen = true;
			DropdownMenu.PlacementTarget = sender as Button;
			DropdownMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
		}
	}
}