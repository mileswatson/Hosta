using System;
using System.Collections.Generic;
using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.Components
{
	public class PostViewModel : ObservableObject
	{
		public string User { get; init; }

		public string ID { get; init; }

		public ProfileViewModel Profile { get; init; }

		private string _content = "";

		public string Content
		{
			get => _content;
			private set
			{
				_content = value;
				NotifyPropertyChanged(nameof(Content));
			}
		}

		private ImageViewModel? _image = null;

		public ImageViewModel? Image
		{
			get => _image;
			private set
			{
				_image = value;
				NotifyPropertyChanged(nameof(Image));
			}
		}

		private DateTime _timePosted = DateTime.MinValue;

		public DateTime TimePosted
		{
			get => _timePosted;
			set
			{
				_timePosted = value;
				NotifyPropertyChanged(nameof(TimePosted));
			}
		}

		public List<ContextMenuItem<PostViewModel>> MenuItems { get; init; }

		public PostViewModel(string user, string id)
		{
			User = user;
			ID = id;
			Profile = new ProfileViewModel(user);
			MenuItems = new();
		}

		public PostViewModel(string user, string id, List<ContextMenuItem<PostViewModel>> menuItems) : this(user, id)
		{
			MenuItems = menuItems;
		}

		public override async void Update(bool force = false)
		{
			Profile.Update(force);
			var newPost = await Resources!.GetPost(User, ID, force);
			if (newPost.Content != Content) Content = newPost.Content;
			if (newPost.TimePosted != TimePosted) TimePosted = newPost.TimePosted;
			if (newPost.ImageHash == "") Image = null;
			else if (Image is null || newPost.ImageHash != Image.Hash)
			{
				Image = new ImageViewModel(User, newPost.ImageHash);
				Image.Update(force);
			}
		}
	}
}