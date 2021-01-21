using ClientWPF.ViewModels.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.HomeTab
{
	public class ViewModel : ObservableObject
	{
		public PostFeedViewModel Feed { get; init; }

		public PeopleViewModel Friends { get; init; }

		public ViewModel()
		{
			Feed = new PostFeedViewModel(Resources!.Self, new());
			Friends = new PeopleViewModel();
		}

		public override Task UpdateAsync(bool force)
		{
			Feed.Update(force);
			Friends.Update(force);
			return Task.CompletedTask;
		}
	}
}