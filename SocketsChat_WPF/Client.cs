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
        private TcpClient _client = new TcpClient();
        private Guid ClientGuid { get; set; }
        private NetworkStream Stream => _client?.GetStream();
        public event Action<MessageData> MessageReceived;
        public event Action<List<string>> UserListReceived;
        public event Action<string,string> UserNameChanged;
        public bool IsConnected => _client.Connected;



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

        void ProcessMessage(MessageData msg)
        {
            switch (msg.CmdCommand)
            {
                case MessageData.Command.Login:
                    ClientGuid = msg.Id;
                    UserName = msg.Message;
                    msg.Message = "Connected";
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

        private void OnUserListReceived(string userList)
        {
            var userDictionary = userList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(pair => pair.Split('=')).ToDictionary(pair => pair[0], pair => pair[1]);

            UserListReceived?.Invoke(userDictionary);
        }

        private void ProcessChangeName(MessageData msg)
        {
            UserName = msg.Error ? UserName : msg.Message;
        }

        public void Close()
        {
            WriteMessageAsync(new MessageData {CmdCommand = MessageData.Command.Logout, Id = ClientGuid });
        }

        public async void WriteMessageAsync(MessageData message)
        {
            if (Stream == null)
                throw new Exception("WriteMessageAsync() : Stream is null");
            message.Id = ClientGuid;
            byte[] bytes = message.ToByteArray();
            await Stream.WriteAsync(bytes, 0, bytes.Length);
        }

        
        
    }
}
