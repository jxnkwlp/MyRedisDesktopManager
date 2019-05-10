using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyRedisDesktopManager
{
	public class Command : ICommand
	{
		private readonly Action<object> _commandAction;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public Command()
		{
		}

		public Command(Action<object> action)
		{
			this._commandAction = action;
		}

		public virtual bool CanExecute(object parameter)
		{
			return true;
		}

		public virtual void Execute(object parameter)
		{
			_commandAction?.Invoke(parameter);
		}
	}

	public class Command<T> : ICommand
	{
		private readonly Action<T> _commandAction;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public Command(Action<T> action)
		{
			this._commandAction = action;
		}

		public virtual bool CanExecute(object parameter)
		{
			return true;
		}

		public virtual void Execute(object parameter)
		{
			_commandAction.Invoke((T)parameter);
		}
	}
}
