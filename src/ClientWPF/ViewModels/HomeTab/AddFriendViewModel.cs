using Hosta.API;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.HomeTab
{
	public class AddFriendViewModel : ObservableObject
	{
		private string _name = "";

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				NotifyPropertyChanged(nameof(Name));
			}
		}

		private string _id = "";

		public string ID
		{
			get => _id;
			set
			{
				_id = value;
				NotifyPropertyChanged(nameof(ID));
			}
		}

		public ICommand Submit { get; init; }

		public ICommand Cancel { get; init; }

		public AddFriendViewModel(Action done)
		{
			Cancel = new RelayCommand((object? _) => done());
			Submit = new RelayCommand(async (object? _) =>
			{
				try
				{
					await Resources!.SetFriend(ID, Name, false);
					done();
				}
				catch (APIException e)
				{
					Env.Alert($"Could not add friend! {e.Message}");
				}
				catch
				{
					Env.Alert($"Could not add friend!");
				}
			});
		}

		public override Task UpdateAsync(bool force)
		{
			return Task.CompletedTask;
		}
	}
}