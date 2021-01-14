using ClientWPF.ViewModels.ImagesTab;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views.ImagesTab
{
	/// <summary>
	/// Interaction logic for View.xaml
	/// </summary>
	public partial class View : UserControl
	{
		public View()
		{
			InitializeComponent();
		}

		public void CopyMenuItem_Clicked(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("here!");
			var menuItem = sender as MenuItem;
			var hash = (menuItem?.Tag ?? "") as string;
			Clipboard.SetText(hash);
		}

		public void UploadButton_Clicked(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog();

			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				var vm = (ViewModel)DataContext;
				vm.Upload(dialog.FileName);
			}
		}
	}
}