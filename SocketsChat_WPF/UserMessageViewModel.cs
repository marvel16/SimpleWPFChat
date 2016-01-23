using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketsChat_WPF
{
    public class UserMessageViewModel : BaseViewModel
    {
        private string _id = String.Empty;
        private string _messageTime = String.Empty;
        private string _message = String.Empty;
        private string _status = String.Empty;


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

        
        public string MessageTime
        {
            get { return _messageTime; }
            set
            {
                if (_messageTime == value)
                    return;
                _messageTime = value;
                OnPropertyChanged(MessageTime);
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
