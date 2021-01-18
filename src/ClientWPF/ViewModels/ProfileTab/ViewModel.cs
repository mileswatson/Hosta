using ClientWPF.Models.Data;
using System.Threading.Tasks;

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

		public override Task UpdateAsync(bool force)
		{
			VM.Update(force);
			return Task.CompletedTask;
		}
	}
}