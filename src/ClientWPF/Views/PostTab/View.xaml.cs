using ClientWPF.ViewModels.PostTab;
using ClientWPF.Views.ImagePicker;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views.PostTab
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

		public void SelectImageButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (DataContext is null) return;
			ViewModel vm = DataContext as ViewModel ?? throw new NullReferenceException();
			var picker = new ImagePickerWindow(vm.SetImageHash);
			picker.ShowDialog();
		}
	}
}