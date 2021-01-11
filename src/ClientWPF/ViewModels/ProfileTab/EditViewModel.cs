using static ClientWPF.Models.ResourceManager;
using static ClientWPF.ApplicationEnvironment;
using ClientWPF.Models.Data;
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

		public BitmapImage Avatar { get; private set; }

		public ICommand RemoveAvatar { get; init; }

		private byte[] avatarBytes;

		private void SetAvatarBytes(byte[] bytes)
		{
			try
			{
				Avatar = Profile.ImageFromBytes(bytes);
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
			Avatar = Profile.DefaultImage;
			NotifyPropertyChanged(nameof(Avatar));
		}

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
			Avatar = profile.Avatar;
			avatarBytes = profile.AvatarBytes;
			RemoveAvatar = new RelayCommand((object? _) => ClearAvatarBytes());
			Save = new RelayCommand(async (object? _) =>
			{
				try
				{
					await Resources!.SetProfile(Name, Tagline, Bio, avatarBytes);
					OnDone(true);
				}
				catch (RPException e)
				{
					Env.Alert($"Could not update profile! {e.Message}");
				}
				catch
				{
					Env.Alert("Could not update profile!");
				}
			});
			CancelEditing = new RelayCommand((object? _) => OnDone(false));
		}

		public override void Update(bool force)
		{
		}
	}
}