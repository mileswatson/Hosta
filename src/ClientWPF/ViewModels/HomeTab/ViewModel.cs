using ClientWPF.ViewModels.Components;
using System.Collections.Generic;

namespace ClientWPF.ViewModels.HomeTab
{
	public class ViewModel : ObservableObject
	{
		private List<ObservableObject> _posts = new();

		public List<ObservableObject> Posts
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
			var newList = new List<ObservableObject>();
			for (int i = 0; i < 100; i++)
			{
				newList.Add(new PostViewModel("", ""));
			}
			Posts = newList;
		}

		public override void Update(bool force)
		{
		}
	}
}