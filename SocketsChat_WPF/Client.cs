using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CustomNetworkExtensions;
using CustomNetworkExtensions.Annotations;

namespace SocketsChat_WPF
{
    public class Client
    {
        public Guid UserGuid { get; set; }
        public bool IsConnected => _client.Connected;
        public event Action<MessageData> MessageReceived;
        public event Action<string,string> UserNameChanged;
        public event Action<Dictionary<string,string>> UserListReceived;
        private TcpClient _client = new TcpClient();
        private NetworkStream Stream => _client?.GetStream();
        private Dictionary<string, string> UserNameDictionary = new Dictionary<string, string>();



        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName)
                    return;
                string oldValue = _userName;
                _userName = value;
                UserNameChanged?.Invoke(oldValue, _userName);
            }
        }


        public async Task Connect(string ip, int port)
        {
            _client?.Close();
            _client = new TcpClient();

            await _client.ConnectAsync(ip, port).ConfigureAwait(false);

            await ReadMessageAsync().ConfigureAwait(false);
        }


       

        void ProcessMessage(MessageData msg)
        {
            switch (msg.CmdCommand)
            {
                case MessageData.Command.Login:
                    OnUserLogin(msg);
                    MessageReceived?.Invoke(msg);
                    break;
                case MessageData.Command.ChangeName:
                    ProcessChangeName(msg);
                    break;
                case MessageData.Command.Message:
                    MessageReceived?.Invoke(msg);
                    break;
                case MessageData.Command.List:
                    OnUserListReceived(msg.Message);
                    break;
                default:
                    break;
            }

        }

        private void OnUserLogin(MessageData msg)
        {
            UserGuid = msg.Id;
            OnUserListReceived(msg.Message);
            string userName;
            if (!UserNameDictionary.TryGetValue(msg.Id.ToString(), out userName))
                userName = "Doesn't exist";
            UserName = userName;
            msg.Message = "Connected";
        }

        private void OnUserListReceived(string userList)
        {
            UserNameDictionary = userList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(entry => entry.Split('=')).ToDictionary(entry => entry[0], entry => entry[1]);
            UserListReceived?.Invoke(UserNameDictionary);
        }

        private void ProcessChangeName(MessageData msg)
        {
            UserName = msg.Error ? UserName : msg.Message;
        }

        public void Close()
        {
            WriteMessageAsync(new MessageData {CmdCommand = MessageData.Command.Logout, Id = UserGuid });
        }

        public async Task ReadMessageAsync()
        {
            if (Stream == null)
                throw new Exception("ReadMessageAsync(): Stream is null.");
            MessageData msg = null;
            while (_client.Connected)
            {
                int headerLength = sizeof(int);
                byte[] header = await Stream.ReadMessageFromStreamAsync(headerLength).ConfigureAwait(false);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] message = await Stream.ReadMessageFromStreamAsync(messageLength).ConfigureAwait(false);

                msg = message.ByteArrayToMessage();

                ProcessMessage(msg);
            }
        }

        public async void WriteMessageAsync(MessageData message)
        {
            if (Stream == null)
                throw new Exception("WriteMessageAsync() : Stream is null");
            
            byte[] bytes = message.ToByteArray();
            await Stream.WriteAsync(bytes, 0, bytes.Length);
        }

        
        
    }
}
