using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Client.Commands;
using Client.Models;
using NetworkExtensions;
using NetworkExtensions.Entities;

namespace Client.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private ClientModel _clientModel;
        private string _userConfig = "options.xml";
        private Options _options = new Options();
        public ObservableCollection<UserMessageViewModel> UserMessages { get; } = new ObservableCollection<UserMessageViewModel>();

        private readonly object _userMessagesLock = new object();

        private UserOptionsViewModel _optionsViewModel;

        private bool Connected => _clientModel.IsConnected;
        private Dictionary<string, string> UserNameDict => _clientModel.UserNameDictionary;

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
            }
        }

       #region Commands

        public ICommand SendCmd { get; }
        public ManualRelayCommand ConnectCmd { get; }

        public ICommand UserOptionsCmd { get; }

        public ICommand EnterNewLineCmd { get; }
        #endregion

        public ClientViewModel(ClientModel clientModel)
        {
            LoadConfig();
            
            _clientModel = clientModel;
            _clientModel.MessageReceived += OnMessageDataReceived;
            _clientModel.UserNameChanged += OnUserNameChanged;
            _clientModel.OnLogin += OnLogin;

            BindingOperations.EnableCollectionSynchronization(UserMessages, _userMessagesLock);

            ConnectCmd = new ManualRelayCommand(
                async x =>
                {
                    try
                    {
                        await _clientModel.Connect(_options.Ip, int.Parse(_options.Port));
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                        AddSystemMessage("Couldn't connect: " + e.Message);
                    }
                },
                x => ValidateConnectionInfo());

            SendCmd = new RelayCommand(
                x => SendMessage(),
                x=> !string.IsNullOrWhiteSpace(MessageTextToSend) && Connected);

            EnterNewLineCmd = new TextBoxEnterNewLineCommand();

            UserOptionsCmd = new RelayCommand(x =>
                {
                    var userOptionsDialog = new UserOptions();
                    _optionsViewModel = new UserOptionsViewModel(_options, o =>
                    {
                        OnUserOptionsSave();
                        userOptionsDialog.Close();
                    });
                    userOptionsDialog.DataContext = _optionsViewModel;
                    userOptionsDialog.Show();
                });
                
        }

        private void LoadConfig()
        {
            try
            {
                Options opts = null;
                ConfigSerializer.Deserialize(ref opts, _userConfig);
                if (opts != null)
                    _options = opts;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            _options.PropertyChanged += (sender, args) => ConnectCmd.RaiseCanExecuteChanged();
        }

        private void SendMessage()
        {
            var msg = new MessageData
            {
                Id = _clientModel.UserId,
                Message = MessageTextToSend.TrimEnd(),
                Command = Command.Message,
                MessageTime = DateTime.Now,
            };

            var msgItem = ConvertMessageDataToViewModel(msg);
            UserMessages.Add(msgItem);
            MessageTextToSend = string.Empty;
            _clientModel.WriteMessageAsync(msg);

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
                ConfigSerializer.Serialize(_options, _userConfig);
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
            _clientModel.ChangeUserNameRequest(newName);
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




}
