using System;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class ContextMenuItem<T> where T : class
	{
		public string Header { get; init; }
		public ICommand Command { get; init; }

		public ContextMenuItem(string header, Action<T?> action)
		{
			Header = header;
			Command = new RelayCommand((object? o) => action(o as T));
		}
	}
}