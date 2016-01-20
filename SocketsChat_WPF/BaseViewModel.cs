using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SocketsChat_WPF.Annotations;

namespace SocketsChat_WPF
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserMessageViewModel : BaseViewModel
    {
        private string _message = String.Empty;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value)
                    return;
                _message = value;
                OnPropertyChanged(_message);
            }
        }

        private string _id = String.Empty;
        public string Id
        {
            get { return _id; }
            set
            {
                if (_message == value)
                    return;
                _id = value;
                OnPropertyChanged(_id);
            }
        }

        private string _status = String.Empty;
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;
                _status = value;
                OnPropertyChanged(_status);
            }
        }
    }

    public class UserMessagesViewModel
    {
        public ObservableCollection<UserMessageViewModel> UserMessages { set; get; }


        public UserMessagesViewModel(List<UserMessageViewModel> messages)
        {
            UserMessages = new ObservableCollection<UserMessageViewModel>(messages);
        }
    }
    
    

}
