﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		private InfoViewModel profileInfo;

		private EditViewModel profileEdit;

		public ViewModel()
		{
			profileInfo = new(StartEditing);
			profileEdit = new(StopEditing);
			VM = profileInfo;
		}

		public void StartEditing()
		{
			VM = profileEdit;
		}

		public void StopEditing()
		{
			VM = profileInfo;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}