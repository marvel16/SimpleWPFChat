using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomNetworkExtensions;

namespace SocketsChat_WPF
{
    public class UserMessageViewModel : BaseViewModel
    {
        private string _id = String.Empty;
        private string _messageTime = String.Empty;
        private string _message = String.Empty;
        private string _status = String.Empty;
        private string _command = String.Empty;

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

        public string Command
        {
            get { return _command; }
            set
            {
                if (_command == value)
                    return;
                _command = value;
                OnPropertyChanged(Command);
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
        private Client _client;
        public ObservableCollection<UserMessageViewModel> UserMessages { set; get; }

        public SendCommand Send;

        public UserMessagesViewModel(Client client)
        {
            _client = client;
            _client.MessageReceived += OnMessageDataReceived;
            _client.Connect("localhost", 50000);
        }

        public void OnMessageDataReceived(MessageData message)
        {
            UserMessages.Add(ConvertMessageDataToViewModel(message));
        }


        private UserMessageViewModel ConvertMessageDataToViewModel(MessageData msgData)
        {
            return new UserMessageViewModel()
            {
                Message = msgData.Message,
                Command = msgData.CmdCommand.ToString(),
                Id = msgData.Id.ToString(),
                Status = msgData.Status.ToString(),
                MessageTime = msgData.MessageTime.ToString(CultureInfo.InvariantCulture),
            };
        }

        
    }

    public class SendCommand : ICommand
    {
        public Action SendAction;

        public bool CanExecute(object parameter)
        {
            
        }

        public void Execute(object parameter)
        {
            var client = (Client) parameter;
        }

        public event EventHandler CanExecuteChanged;
    }
}
