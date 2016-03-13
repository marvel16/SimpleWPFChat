using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using NetworkExtensions;

namespace SocketsChat_WPF
{
    public class UserOptionsViewModel : ViewModelBase
    {
        public SaveOptionsCmd SaveCmd { get; } = new SaveOptionsCmd();

        public Options Opts;

        public string UserName
        {
            get { return Opts.UserName; }
            set
            {
                if (Opts.UserName == value)
                    return;
                Opts.UserName = value;
                OnPropertyChanged();
            }
        }

        public string Ip
        {
            get { return Opts.Ip; }
            set
            {
                if (Opts.Ip == value)
                    return;
                Opts.Ip = value;
                OnPropertyChanged();
            }
        }

        public string Port
        {
            get { return Opts.Port; }
            set
            {
                if (Opts.Port == value)
                    return;
                Opts.Port = value;
                OnPropertyChanged();
            }
        }

        public UserOptionsViewModel(Options opts)
        {
            Opts = opts;
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
