using System;
using System.ComponentModel;
using System.Threading.Tasks;

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

		public abstract Task UpdateAsync(bool force);

		private enum UpdateStatus
		{
			NotUpdating = 0,
			Updating = 1,
			ForceUpdating = 2
		}

		private UpdateStatus status = UpdateStatus.NotUpdating;

		public async void Update(bool force)
		{
			var lastStatus = status;
			var thisStatus = force ? UpdateStatus.ForceUpdating : UpdateStatus.Updating;
			if (thisStatus <= lastStatus) return;
			status = thisStatus;
			await UpdateAsync(force);
			if (status == thisStatus) status = UpdateStatus.NotUpdating;
			else if (thisStatus == UpdateStatus.ForceUpdating) throw new Exception();
		}
	}
}