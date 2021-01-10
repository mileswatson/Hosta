using ClientWPF.ViewModels.ProfileTab;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views.ProfileTab
{
	/// <summary>
	/// Interaction logic for EditView.xaml
	/// </summary>
	public partial class EditView : UserControl
	{
		public EditView()
		{
			InitializeComponent();
		}

		public void SelectButton_Clicked(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog
			{
			};
			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				var vm = (EditViewModel)DataContext;
				vm.SetAvatarFile(dialog.FileName);
			}
		}
	}
}