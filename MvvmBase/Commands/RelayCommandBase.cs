using System;
using System.Windows.Input;

namespace MvvmBase.Commands
{
    /// <summary>
    /// Implementation of RelayCommand
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommandBase : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

#region Constructors
        public RelayCommandBase(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        
    }
}