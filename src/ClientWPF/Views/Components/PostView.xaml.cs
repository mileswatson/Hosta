using System.Windows;
using System.Windows.Controls;

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