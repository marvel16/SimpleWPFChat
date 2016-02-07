using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CustomNetworkExtensions;
using CustomNetworkExtensions.Annotations;

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
                OnPropertyChanged(Message);
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
                OnPropertyChanged(Id);
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
                OnPropertyChanged(Status);
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

        public string UserName { get; set; } = String.Empty;
    }

    public class ClientViewModel : BaseViewModel
    {
        private Client _client;
        public ObservableCollection<UserMessageViewModel> UserMessages { get; } = new ObservableCollection<UserMessageViewModel>();
        private ObservableCollection<string> _userList = new ObservableCollection<string>();
        
        public ObservableCollection<string> UserList
        {
            get { return _userList; }
            set
            {
                if (value == _userList)
                    return;
                _userList = value;
                OnPropertyChanged(nameof(UserList));
            }
        }

        private string _messageText;
        public string MessageTextToSend
        {
            get { return _messageText; }
            set
            {
                if (value == _messageText)
                    return;
                _messageText = value;
                OnPropertyChanged(MessageTextToSend);
                SendCmd?.OnTextChanged();
            }
        }

        private string _ip;

        public string Ip
        {
            get { return _ip; }
            set
            {
                if (_ip == value)
                    return;
                _ip = value;
                ConnectCmd?.OnAddressChanged();
            }
        }

        private string _port;
        public string Port
        {
            get { return _port; }
            set
            {
                if (_port == value)
                    return;
                _port = value;
                ConnectCmd?.OnAddressChanged();
            }
        }

        #region Commands

        public SendCommand SendCmd { get; }
        public ConnectCommand ConnectCmd { get; }

        #endregion

        public ClientViewModel(Client client)
        {
            _client = client;
            _client.MessageReceived += OnMessageDataReceived;
            _client.UserListReceived += OnUserListReceived;

            //_client.Connect("localhost", 50000);

            ConnectCmd = new ConnectCommand
            {
                CanExecuteAction = () => ValidateIpAndPort(),
                ConnectAction = () => _client.Connect(Ip, int.Parse(Port))
            };

            SendCmd = new SendCommand
            {
                CanExecuteAction = () => !string.IsNullOrEmpty(MessageTextToSend) && _client.IsConnected,
                SendAction = () => SendMessage(),
            };
             
        }

        private void SendMessage()
        {
            var msg = new MessageData
            {
                Message = MessageTextToSend,
                CmdCommand = MessageData.Command.Message,
                MessageTime = DateTime.Now,
            };

            var msgItem = ConvertMessageDataToViewModel(msg);
            msgItem.UserName = "You";
            UserMessages.Add(msgItem);
            MessageTextToSend = string.Empty;
            _client.WriteMessageAsync(msg);

        }

        private void OnMessageDataReceived(MessageData message)
        {
            UserMessages.Add(ConvertMessageDataToViewModel(message));
        }

        private void OnUserListReceived(List<string> uList)
        {
            UserList.Clear();
            uList.ForEach(i => UserList.Add(i));
        }

        private bool ValidateIpAndPort()
        {
            int p;
            IPAddress ip;
            return int.TryParse(Port, out p) && IPAddress.TryParse(Ip, out ip);
        }

        private UserMessageViewModel ConvertMessageDataToViewModel(MessageData msgData)
        {
            return new UserMessageViewModel()
            {
                Message = msgData.Message,
                Command = msgData.CmdCommand.ToString(),
                Id = msgData.Id.ToString(),
                Status = msgData.Status.ToString(),
                MessageTime = msgData.MessageTime.ToShortTimeString(),
            };
        }

        
    }

    public class SendCommand : ICommand
    {
        public Func<bool> CanExecuteAction;
        public Action SendAction;

        public void OnTextChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteAction();
        }

        public void Execute(object parameter)
        {
            SendAction();
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ConnectCommand : ICommand
    {
        public Func<bool> CanExecuteAction;
        public Action ConnectAction;

        public void OnAddressChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        public bool CanExecute(object parameter)
        {
            return CanExecuteAction();
        }

        public void Execute(object parameter)
        {
            ConnectAction();
        }

        public event EventHandler CanExecuteChanged;
    }
}
