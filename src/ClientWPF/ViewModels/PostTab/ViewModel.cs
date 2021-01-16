using System.Windows.Input;
using static ClientWPF.Models.ResourceManager;
using static ClientWPF.ApplicationEnvironment;
using Hosta.RPC;

namespace ClientWPF.ViewModels.PostTab
{
	public class ViewModel : ObservableObject
	{
		private string _content = "";

		public string Content
		{
			get => _content;
			set
			{
				_content = value;
				NotifyPropertyChanged(nameof(Content));
			}
		}

		public ICommand Post { get; init; }

		public ViewModel()
		{
			Post = new RelayCommand(async (object? _) =>
			{
				if (!Env.Confirm("Are you sure you want to post?")) return;
				try
				{
					var id = await Resources!.AddPost(Content, "");
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