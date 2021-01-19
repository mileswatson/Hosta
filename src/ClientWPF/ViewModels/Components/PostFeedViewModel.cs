using Hosta.API.Post;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.Components
{
	public class PostFeedViewModel : ObservableObject
	{
		private List<PostViewModel> _posts = new();

		public List<PostViewModel> Posts
		{
			get => _posts;
			private set
			{
				_posts = value;
				NotifyPropertyChanged(nameof(Posts));
			}
		}

		private readonly List<ContextMenuItem<PostViewModel>> menuItems;

		private readonly string user;

		public PostFeedViewModel(string user, List<ContextMenuItem<PostViewModel>> menuItems)
		{
			this.user = user;
			this.menuItems = menuItems;
		}

		public override async Task UpdateAsync(bool force = false)
		{
			var infoList = await Resources!.GetPostList(user, DateTime.Now - TimeSpan.FromDays(1));
			infoList.Sort(Compare);
			var newList = new List<PostViewModel>();
			foreach (var info in infoList)
			{
				var post = new PostViewModel(user, info.ID, menuItems);
				post.Update(force);
				newList.Add(post);
			}
			Posts = newList;
		}

		public static int Compare(PostInfo x, PostInfo y)
		{
			if (x.TimePosted < y.TimePosted) return 1;
			else if (x.TimePosted > y.TimePosted) return -1;
			else return 0;
		}
	}
}