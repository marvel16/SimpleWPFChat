using System;
using System.Collections.Concurrent;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName)
                    return;
                _userName = value;
                OnPropertyChanged();
            }
        }
    }

    public class ClientViewModel : BaseViewModel
    {
        private Client _client;
        public ObservableCollection<UserMessageViewModel> UserMessages { get; } = new ObservableCollection<UserMessageViewModel>();
        public ConcurrentDictionary<string, string> UserList { get;} = new ConcurrentDictionary<string, string>();
        private readonly object _userMessagesLock = new object();


        private string _messageText;
        public string MessageTextToSend
        {
            get { return _messageText; }
            set
            {
                if (value == _messageText)
                    return;
                _messageText = value;
                OnPropertyChanged();
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
            _client.UserNameChanged += OnUserNameChanged;

            BindingOperations.EnableCollectionSynchronization(UserMessages, _userMessagesLock);

            ConnectCmd = new ConnectCommand
            {
                CanExecuteAction = () => ValidateIpAndPort(),
                ConnectAction = async () => await _client.Connect(Ip, int.Parse(Port))
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
            UserMessages.Add(msgItem);
            MessageTextToSend = string.Empty;
            _client.WriteMessageAsync(msg);

        }

        private void OnMessageDataReceived(MessageData message)
        {
            UserMessages.Add(ConvertMessageDataToViewModel(message));
        }

        private void OnUserNameChanged(string oldName, string newName)
        {
            foreach (var msg in UserMessages.Where(m => m.UserName == oldName))
                msg.UserName = newName;

        }

        private void OnUserListReceived(Dictionary<string,string> userDictionary)
        {
            UserList.Clear();
            foreach (var user in userDictionary)
            {
                UserList[user.Key] = user.Value;
            }
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
                UserName = _client.UserName,
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
