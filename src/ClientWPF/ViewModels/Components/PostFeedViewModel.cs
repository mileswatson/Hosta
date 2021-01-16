using Hosta.API.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			set
			{
				_posts = value;
				NotifyPropertyChanged(nameof(Posts));
			}
		}

		private string user;

		public PostFeedViewModel(string user)
		{
			this.user = user;
		}

		public override async void Update(bool force = false)
		{
			var infoList = await Resources!.GetPostList(user, DateTime.Now - TimeSpan.FromDays(1));
			infoList.Sort(Compare);
			var newList = new List<PostViewModel>();
			foreach (var info in infoList)
			{
				var post = new PostViewModel(user, info.ID);
				post.Update(force);
				newList.Add(post);
			}
			Posts = newList;
		}

		public int Compare(PostInfo x, PostInfo y)
		{
			if (x.TimePosted < y.TimePosted) return 1;
			else if (x.TimePosted > y.TimePosted) return -1;
			else return 0;
		}
	}
}