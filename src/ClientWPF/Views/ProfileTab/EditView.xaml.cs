using ClientWPF.ViewModels.ProfileTab;
using ClientWPF.Views.ImagePicker;
using System;
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

		public void ChangeButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (DataContext is null) return;
			EditViewModel vm = DataContext as EditViewModel ?? throw new NullReferenceException();
			var picker = new ImagePickerWindow(vm.SetAvatarHash);
			picker.ShowDialog();
		}

		public void RemoveButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (DataContext is null) return;
			EditViewModel vm = DataContext as EditViewModel ?? throw new NullReferenceException();
			vm.SetAvatarHash("");
		}
	}
}