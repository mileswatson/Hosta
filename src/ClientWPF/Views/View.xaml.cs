using System.IO;
using System.Windows;
using System.Windows.Controls;
using static ClientWPF.ApplicationEnvironment;

namespace ClientWPF.Views
{
	/// <summary>
	/// Interaction logic for View.xaml
	/// </summary>
	public partial class View : UserControl
	{
		public View()
		{
			InitializeComponent();

			SetAlert(
				(string message) =>
				{
					MessageBox.Show(message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			);

			SetConfirmation(
				(string message) =>
				{
					var result = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					return result == MessageBoxResult.Yes;
				}
			);

			SetDirectoryChecker(
				(string path) => Directory.Exists(path)
			);

			SetFileChecker(
				(string path) => File.Exists(path)
			);

			SetFileReader(
				(string path) => File.ReadAllTextAsync(path)
			);

			SetFileWriter(
				(string path, string data) => File.WriteAllTextAsync(path, data)
			);
		}
	}
}