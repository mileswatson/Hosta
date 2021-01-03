using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	/// <summary>
	/// Creates an instance of a command from an action.
	/// </summary>
	public class RelayCommand : ICommand
	{
		private readonly Action<object?> _execute;

		public RelayCommand(Action<object?> execute)
		{
			_execute = execute;
		}

		public bool CanExecute(object? _) => true;

		public event EventHandler? CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object? parameter)
		{
			_execute(parameter);
		}
	}
}