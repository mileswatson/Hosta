using System.ComponentModel;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class ViewModel : INotifyPropertyChanged
	{
		private object _vm;

		public object VM
		{
			get
			{
				return _vm;
			}
			set
			{
				_vm = value;
				NotifyPropertyChanged(nameof(VM));
			}
		}

		private readonly InfoViewModel profileInfo;

		private readonly EditViewModel profileEdit;

		public ViewModel()
		{
			profileInfo = new(StartEditing);
			profileEdit = new(StopEditing);
			_vm = profileInfo;
		}

		public void StartEditing()
		{
			VM = profileEdit;
		}

		public void StopEditing()
		{
			VM = profileInfo;
			//profileInfo.Update();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}