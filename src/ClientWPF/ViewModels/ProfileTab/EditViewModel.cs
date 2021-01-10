using static ClientWPF.Models.ResourceManager;
using static ClientWPF.ApplicationEnvironment;
using ClientWPF.Models.Components;
using System;
using System.Windows.Input;
using Hosta.RPC;
using System.Windows.Media.Imaging;

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

		private byte[] avatarBytes = Array.Empty<byte>();

		private void SetAvatarBytes(byte[] bytes, bool checkValid = true)
		{
			try
			{
				Tools.GetImage(bytes);
				avatarBytes = bytes;
				NotifyPropertyChanged(nameof(Avatar));
			}
			catch
			{
				Env.Alert("Image could not be decoded!");
			}
		}

		private void ClearAvatarBytes()
		{
			avatarBytes = Array.Empty<byte>();
			NotifyPropertyChanged(nameof(Avatar));
		}

		public BitmapImage Avatar
		{
			get => Tools.TryGetImage(avatarBytes);
		}

		public ICommand RemoveAvatar { get; init; }

		public async void SetAvatarFile(string path)
		{
			try
			{
				SetAvatarBytes(await Env.ReadFileRaw(path));
			}
			catch
			{
				Env.Alert("Could not read image!");
			}
		}

		public EditViewModel(Action<bool> OnDone, Profile profile)
		{
			Name = profile.DisplayName;
			Tagline = profile.Tagline;
			Bio = profile.Bio;
			avatarBytes = profile.Avatar;
			RemoveAvatar = new RelayCommand((object? _) => ClearAvatarBytes());
			Save = new RelayCommand(async (object? _) =>
			{
				try
				{
					await Resources!.SetProfile(Name, Tagline, Bio, avatarBytes);
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
				OnDone(true);
			});
			CancelEditing = new RelayCommand((object? _) => OnDone(false));
		}

		public override void Update(bool force)
		{
		}
	}
}