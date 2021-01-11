using ClientWPF.ViewModels.Components;
using System.Collections.ObjectModel;

namespace ClientWPF.ViewModels.HomeTab
{
	public class ViewModel : ObservableObject
	{
		private ObservableCollection<ObservableObject> _posts = new();

		public ObservableCollection<ObservableObject> Posts
		{
			get => _posts;
			set
			{
				_posts = value;
				NotifyPropertyChanged(nameof(Posts));
			}
		}

		public ViewModel()
		{
			Posts = new();
			for (int i = 0; i < 100; i++)
			{
				Posts.Add(new PostViewModel());
			}
		}

		public override void Update(bool force)
		{
		}
	}
}