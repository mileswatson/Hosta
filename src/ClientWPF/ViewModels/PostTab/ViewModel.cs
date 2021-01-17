using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;
using static ClientWPF.ApplicationEnvironment;
using Hosta.RPC;
using ClientWPF.ViewModels.Components;

namespace ClientWPF.ViewModels.PostTab
{
	public class ViewModel : ObservableObject
	{
		private string _content = "Make a post...";

		public string Content
		{
			get => _content;
			set
			{
				_content = value;
				NotifyPropertyChanged(nameof(Content));
			}
		}

		private string avatarHash = "";

		private ImageViewModel? _image = null;

		public ImageViewModel? Image
		{
			get => _image;
			set
			{
				_image = value;
				NotifyPropertyChanged(nameof(Image));
			}
		}

		public void SetImageHash(string hash)
		{
			avatarHash = hash;
			Image = new ImageViewModel(Resources!.Self, hash);
			Image.Update(false);
		}

		public ICommand RemoveImage { get; init; }

		public ICommand Post { get; init; }

		public ViewModel()
		{
			RemoveImage = new RelayCommand((object? _) =>
			{
				avatarHash = "";
				Image = null;
			});
			Post = new RelayCommand(async (object? _) =>
			{
				if (!Env.Confirm("Are you sure you want to post?")) return;
				try
				{
					var id = await Resources!.AddPost(Content, avatarHash);
					Env.Alert($"Posted! ID = {id}");
					Content = "";
				}
				catch (RPException e)
				{
					Env.Alert($"Could not add post! {e.Message}");
				}
				catch
				{
					Env.Alert("Could not add post!");
				}
			});
		}

		public override void Update(bool force)
		{
		}
	}
}