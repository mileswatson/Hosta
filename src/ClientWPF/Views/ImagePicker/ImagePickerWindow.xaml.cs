using ClientWPF.ViewModels.Components;
using System;
using System.Windows;

namespace ClientWPF.Views.ImagePicker
{
	/// <summary>
	/// Interaction logic for ImagePickerWindow.xaml
	/// </summary>
	public partial class ImagePickerWindow : Window
	{
		private Action<string> select;

		public ImagePickerWindow(Action<string> select)
		{
			InitializeComponent();
			this.select = select;
			var picker = new ImagePickerViewModel(Select);
			picker.Update(false);
			view.DataContext = picker;
		}

		public void Select(string hash)
		{
			Close();
			select(hash);
		}
	}
}