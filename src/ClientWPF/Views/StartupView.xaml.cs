using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views
{
	/// <summary>
	/// Interaction logic for StartupView.xaml
	/// </summary>
	public partial class StartupView : UserControl
	{
		public StartupView()
		{
			InitializeComponent();
		}

		public void SelectFolderButton_Clicked(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true
			};
			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				FolderPathTextbox.Text = dialog.FileName;
			}
		}
	}
}