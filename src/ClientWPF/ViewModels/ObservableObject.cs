using System.ComponentModel;

namespace ClientWPF.ViewModels
{
	public abstract class ObservableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is null) return;
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public abstract void Update(bool force);
	}
}