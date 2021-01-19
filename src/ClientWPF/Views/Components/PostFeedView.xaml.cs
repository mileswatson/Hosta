using System.Windows;
using System.Windows.Controls;

namespace ClientWPF.Views.Components
{
	/// <summary>
	/// Interaction logic for PostFeedView.xaml
	/// </summary>
	public partial class PostFeedView : UserControl
	{
		public PostFeedView()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty PostContextMenuProperty = DependencyProperty.Register(
			nameof(PostContextMenu), typeof(ContextMenu),
			typeof(PostFeedView)
		);

		public ContextMenu PostContextMenu
		{
			get { return (ContextMenu)GetValue(PostContextMenuProperty); }
			set { SetValue(PostContextMenuProperty, value); }
		}
	}
}