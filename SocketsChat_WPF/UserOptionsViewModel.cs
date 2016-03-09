using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SocketsChat_WPF
{
    public class UserOptionsViewModel : BaseViewModel
    {
        public SaveOptionsCmd SaveCmd { get; } = new SaveOptionsCmd();

        public Options _opts = new Options();

        public string UserName
        {
            get { return _opts.UserName; }
            set
            {
                if (_opts.UserName == value)
                    return;
                _opts.UserName = value;
                OnPropertyChanged();
            }
        }

    }

    public class SaveOptionsCmd : ICommand
    {
        public Action SaveOptionsAction;
        public Action OnClose;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SaveOptionsAction();
            OnClose?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }
}
