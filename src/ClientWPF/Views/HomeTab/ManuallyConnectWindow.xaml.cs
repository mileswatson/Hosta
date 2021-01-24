using ClientWPF.ViewModels.HomeTab;
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
using System.Windows.Shapes;

namespace ClientWPF.Views.HomeTab
{
	/// <summary>
	/// Interaction logic for ManuallyConnectWindow.xaml
	/// </summary>
	public partial class ManuallyConnectWindow : Window
	{
		private Action<ManuallyConnectWindow> submit;

		public ManuallyConnectWindow(Action<ManuallyConnectWindow> submit)
		{
			InitializeComponent();
			this.submit = submit;
		}

		public void SubmitButton_Clicked(object sender, RoutedEventArgs e)
		{
			submit(this);
		}

		public void CancelButton_Clicked(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}