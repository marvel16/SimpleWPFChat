using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Client.Commands.RelayCommand" />
    public class ManualRelayCommand : RelayCommand
    {
        public ManualRelayCommand(Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute)
        {
        }

        public ManualRelayCommand(Action<object> execute) : base(execute)
        {
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteEventHandler?.Invoke(null, null);
        }

        event EventHandler CanExecuteEventHandler;
    }
}
