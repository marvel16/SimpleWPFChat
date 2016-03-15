using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CustomNetworkExtensions;
using NetworkExtensions;

namespace SocketsChat_WPF
{
    public class UserMessageViewModel : ViewModelBase
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

    public class ClientViewModel : ViewModelBase
    {
        private Client _client;
        private string _userConfig = "options.xml";
        private Options _options = new Options();
        public ObservableCollection<UserMessageViewModel> UserMessages { get; } = new ObservableCollection<UserMessageViewModel>();

        private readonly object _userMessagesLock = new object();

        private UserOptionsViewModel _optionsViewModel;

        private bool Connected => _client.IsConnected;
        private Dictionary<string, string> UserNameDict => _client.UserNameDictionary;

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

       #region Commands

        public SendCommand SendCmd { get; } = new SendCommand();
        public ConnectCommand ConnectCmd { get; } = new ConnectCommand();

        public UserOptionsCommand UserOptionsCmd { get; } = new UserOptionsCommand();

        public SendCommand EnterNewLineCmd { get; } = new SendCommand();
        #endregion

        public ClientViewModel(Client client)
        {
            LoadConfig();
            
            _client = client;
            _client.MessageReceived += OnMessageDataReceived;
            _client.UserNameChanged += OnUserNameChanged;
            _client.OnLogin += OnLogin;

            BindingOperations.EnableCollectionSynchronization(UserMessages, _userMessagesLock);

            ConnectCmd.CanExecuteAction = ValidateConnectionInfo;
            ConnectCmd.ConnectAction = async () =>
            {
                try
                {
                    await _client.Connect(_options.Ip, int.Parse(_options.Port));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    AddSystemMessage("Couldn't connect: " + e.Message);
                }
            };

            SendCmd.CanExecuteAction = () => !string.IsNullOrEmpty(MessageTextToSend) && Connected;
            SendCmd.SendAction = SendMessage;

            EnterNewLineCmd.CanExecuteAction = () => true;

            UserOptionsCmd.UserOptionsAction = () =>
            {
                var userOptionsDialog = new UserOptions();
                _optionsViewModel = new UserOptionsViewModel(_options);
                _optionsViewModel.SaveCmd.SaveOptionsAction = OnUserOptionsSave;
                _optionsViewModel.SaveCmd.OnClose = () => userOptionsDialog.Close();
                userOptionsDialog.DataContext = _optionsViewModel;
                userOptionsDialog.Show();
            };
        }

        private void LoadConfig()
        {
            try
            {
                Options opts = null;
                CfgMan.Deserialize(ref opts, _userConfig);
                if (opts != null)
                    _options = opts;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            _options.PropertyChanged += (sender, args) => ConnectCmd.OnConnectOptionsChanged();
        }

        private void SendMessage()
        {
            var msg = new MessageData
            {
                Id = _client.UserId,
                Message = MessageTextToSend,
                Command = Command.Message,
                MessageTime = DateTime.Now,
            };

            var msgItem = ConvertMessageDataToViewModel(msg);
            UserMessages.Add(msgItem);
            MessageTextToSend = string.Empty;
            _client.WriteMessageAsync(msg);

        }

        private void OnLogin()
        {
            AddSystemMessage("You have joined the chat");
            ChangeUserName(_options.UserName);
        }

        private void OnUserOptionsSave()
        {
            ChangeUserName(_options.UserName);

            try
            {
                CfgMan.Serialize(_options, _userConfig);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        private void OnMessageDataReceived(MessageData message)
        {
            if (message.Error)
                AddSystemMessage(message.Message);
            else
                UserMessages.Add(ConvertMessageDataToViewModel(message));
        }

        private void OnUserNameChanged(string oldName, string newName)
        {
            foreach (var msg in UserMessages.Where(m => m.UserName == oldName))
                msg.UserName = newName;
        }

        private void ChangeUserName(string newName)
        {
            _client.ChangeUserNameRequest(newName);
        }


        private bool ValidateConnectionInfo()
        {
            int p;
            IPAddress ip;
            
            return !Connected && int.TryParse(_options.Port, out p) && IPAddress.TryParse(_options.Ip, out ip);
        }

        private UserMessageViewModel ConvertMessageDataToViewModel(MessageData msgData)
        {
            string userName;

            if (!UserNameDict.TryGetValue(msgData.Id.ToString(), out userName))
                userName = "System";

            return new UserMessageViewModel()
            {
                Message = msgData.Message,
                Command = msgData.Command.ToString(),
                Status = msgData.Status.ToString(),
                MessageTime = msgData.MessageTime.ToShortTimeString(),
                UserName = userName,
            };
        }

        private void AddSystemMessage(string message)
        {
            var msg = new UserMessageViewModel
            {
                Message = message,
                MessageTime = DateTime.Now.ToShortTimeString(),
                UserName = "System",
            };
            UserMessages.Add(msg);
        }

    }


#region Commands

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
            var txtBox = parameter as TextBox;
            if (txtBox != null)
            {
                txtBox.Text += Environment.NewLine;
                txtBox.CaretIndex = txtBox.Text.Length - 1;
            }

            SendAction?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }

    public class UserOptionsCommand : ICommand
    {
        public Action UserOptionsAction;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            UserOptionsAction?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ConnectCommand : ICommand
    {
        public Func<bool> CanExecuteAction;
        public Action ConnectAction;


        public void OnConnectOptionsChanged()
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
#endregion
}
