using System;
using MvvmBase.Properties;

namespace MvvmBase.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="RelayCommand" />
    public class ManualRelayCommand : RelayCommandBase
    {
        public ManualRelayCommand(Action<object> execute, Predicate<object> canExecute = null) : base(execute, canExecute)
        {
        }

        public ManualRelayCommand(Action execute, Predicate<object> canExecute = null) : base(o => execute(), canExecute)
        {
        }

        public ManualRelayCommand(Action execute,[NotNull] Func<bool> canExecute) : base(o => execute(), o => canExecute())
        {
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteEventHandler?.Invoke(null, null);
        }

        event EventHandler CanExecuteEventHandler;
    }
}
