using ClientWPF.ViewModels.HomeTab;
using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views.HomeTab
{
	/// <summary>
	/// Interaction logic for FriendsView.xaml
	/// </summary>
	public partial class PeopleView : UserControl
	{
		public PeopleView()
		{
			InitializeComponent();
		}

		private void AddFriendButton_Click(object sender, RoutedEventArgs e)
		{
			var datacontext = (DataContext as PeopleViewModel);
			if (datacontext is null) return;

			var window = new AddFriendWindow();
			window.DataContext = new AddFriendViewModel(() =>
			{
				window.Close();
				datacontext.Update(false);
			});
			window.ShowDialog();
		}
	}
}