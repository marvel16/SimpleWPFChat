using System;
using MvvmBase.Properties;

namespace MvvmBase.Commands
{
    public class RelayCommand : RelayCommandBase
    {
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null) : base(execute, canExecute)
        {
        }

        public RelayCommand(Action execute, Predicate<object> canExecute = null) : base(o => execute(), canExecute)
        {
        }

        public RelayCommand(Action execute, [NotNull]Func<bool> canExecute) : base(o => execute(), o => canExecute())
        {
        }
    }
}
