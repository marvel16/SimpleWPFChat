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
    public class Client : INotifyPropertyChanged
    {
        private TcpClient _client = new TcpClient();
        private Guid ClientGuid { get; set; }
        private NetworkStream Stream => _client?.GetStream();
        public event Action<MessageData> MessageReceived;
        public event Action<Client> Connected;
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<List<string>> UserListReceived;
        public bool IsConnected => _client.Connected;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string paramName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));
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


        public async Task Connect(string ip, int port)
        {
            _client?.Close();
            _client = new TcpClient();

            await _client.ConnectAsync(ip, port).ConfigureAwait(false);

            Connected?.Invoke(this);
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
                    UserName = msg.Error ? UserName : msg.Message;
                    break;
                case MessageData.Command.Message:
                    MessageReceived?.Invoke(msg);
                    break;
                case MessageData.Command.List:
                    UserListReceived?.Invoke(msg.Message.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).ToList());
                    break;
                default:
                    break;
            }

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
