using System;
using System.Windows.Input;
using Client.Commands;
using Client.Models;

namespace Client.ViewModels
{
    public class UserOptionsViewModel : ViewModelBase
    {
        public RelayCommand SaveCmd { get; }

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

        public UserOptionsViewModel(Options opts, Action<object> execute)
        {
            Opts = opts;
            SaveCmd = new RelayCommand(execute);
        }
    }
}
