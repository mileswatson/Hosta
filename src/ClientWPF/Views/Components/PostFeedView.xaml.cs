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
using System.Windows.Navigation;
using System.Windows.Shapes;

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