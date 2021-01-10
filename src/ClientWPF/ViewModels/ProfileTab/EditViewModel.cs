using static ClientWPF.Models.ResourceManager;
using static ClientWPF.ApplicationEnvironment;
using ClientWPF.Models.Components;
using System;
using System.Windows.Input;
using Hosta.RPC;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class EditViewModel : ObservableObject
	{
		public ICommand Save { get; private set; }
		public ICommand CancelEditing { get; private set; }

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

		private string _tagline = "";

		public string Tagline
		{
			get => _tagline;
			set
			{
				_tagline = value;
				NotifyPropertyChanged(nameof(Tagline));
			}
		}

		private string _bio = "";

		public string Bio
		{
			get => _bio;
			set
			{
				_bio = value;
				NotifyPropertyChanged(nameof(Bio));
			}
		}

		public EditViewModel(Action<bool> OnDone, Profile profile)
		{
			CancelEditing = new RelayCommand((object? _) => OnDone(false));
			Name = profile.DisplayName;
			Tagline = profile.Tagline;
			Bio = profile.Bio;
			Save = new RelayCommand(async (object? _) =>
			{
				try
				{
					await Resources!.SetProfile(Name, Tagline, Bio, profile.Avatar);
				}
				catch (RPException e)
				{
					Env.Alert($"Could not update profile! {e.Message}");
					return;
				}
				catch
				{
					Env.Alert("Could not update profile!");
					return;
				}
				Env.Alert("Updated profile!");
				OnDone(true);
			});
		}

		public override void Update(bool force)
		{
		}
	}
}