using System;
using MvvmBase;

namespace Client.ViewModels
{
    public class UserMessageViewModel : ViewModelBase
    {
        private string _messageTime = string.Empty;
        private string _message = string.Empty;
        private string _status = string.Empty;
        private string _userName = string.Empty;

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public string MessageTime
        {
            get { return _messageTime; }
            set { Set(ref _messageTime, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }
    }
}
