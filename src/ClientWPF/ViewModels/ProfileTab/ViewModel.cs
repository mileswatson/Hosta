using ClientWPF.Models.Data;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class ViewModel : ObservableObject
	{
		private ObservableObject _vm = new InfoViewModel((Profile _) => { });

		public ObservableObject VM
		{
			get
			{
				return _vm;
			}
			set
			{
				_vm = value;
				NotifyPropertyChanged(nameof(VM));
				_vm.Update(false);
			}
		}

		private readonly InfoViewModel profileInfo;

		public ViewModel()
		{
			profileInfo = new(StartEditing);
			VM = profileInfo;
		}

		public void StartEditing(Profile profile)
		{
			VM = new EditViewModel(StopEditing, profile);
		}

		public void StopEditing(bool changed)
		{
			VM = profileInfo;
			VM.Update(changed);
		}

		public override void Update(bool force)
		{
			VM.Update(force);
		}
	}
}