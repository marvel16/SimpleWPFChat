using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Client.Models;
using MvvmBase;
using MvvmBase.Commands;
using MvvmBase.DialogServices;
using NetworkCommon;
using NetworkCommon.Entities;

namespace Client.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private ClientModel _clientModel;
        private string _userConfig = "options.xml";
        private Options _options = new Options();
        public ObservableCollection<UserMessageViewModel> UserMessages { get; } = new ObservableCollection<UserMessageViewModel>();

        private readonly object _userMessagesLock = new object();

        private bool Connected => _clientModel.Connected;
        private Dictionary<string, string> UserNameDict => _clientModel.UserNameDictionary;

        private string _messageText;
        public string MessageTextToSend
        {
            get { return _messageText; }
            set
            {
                Set(ref _messageText, value);
            }
        }

       #region Commands

        public ICommand SendCmd { get; private set; }
        public ManualRelayCommand ConnectCmd { get; private set; }
        public ICommand UserOptionsCmd { get; private set; }
        public ICommand DragAndDropCmd { get; private set; }

        #endregion

        public ClientViewModel(ClientModel clientModel)
        {
            LoadConfig();
            
            _clientModel = clientModel;
            _clientModel.MessageReceived += OnMessageDataReceived;
            _clientModel.UserNameChanged += OnUserNameChanged;
            _clientModel.DownloadFileRequest += OnDownloadRequest;
            _clientModel.OnLogin += OnLogin;

            BindingOperations.EnableCollectionSynchronization(UserMessages, _userMessagesLock);

            InitCommands();
        }

        private void InitCommands()
        {
            ConnectCmd = new ManualRelayCommand(
                async () =>
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
                ValidateConnectionInfo);

            SendCmd = new RelayCommand(SendMessage, x => !string.IsNullOrWhiteSpace(MessageTextToSend) && Connected);

            UserOptionsCmd = new RelayCommand(() => DialogService.Instance.ShowDialog(new UserOptionsViewModel(_options,OnUserOptionsSave)));

            DragAndDropCmd = new RelayCommand(o =>
            {
                IDataObject ido = o as IDataObject;

                string[] files = (string[])ido?.GetData(DataFormats.FileDrop);

                if (files == null || files.Length == 0)
                    return;

                _clientModel.FileTransferRequest(new FileInfo(files[0]));

            },
            o => Connected);
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

        private void OnDownloadRequest(string fileName, string fileSize)
        {
            string formatedSize = long.Parse(fileSize).BytesToString();

            bool result = DialogService.Instance.
                ShowMessageBox($"Do you want to save file\n{fileName} {formatedSize} ?", "File download dialog", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

            if (result)
            {
                var res = DialogService.Instance.SaveFileDialog(fileName);
                if (string.IsNullOrEmpty(res))
                    result = false;
                else
                {
                    fileName = res;
                }
            }

            _clientModel.AcceptFileTransferRequest(result, fileName);
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
