using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkExtensions;
using NetworkExtensions.Entities;

namespace Client.Models
{
    public class ClientModel
    {
        public Guid UserId { get; private set; }
        public bool IsConnected => _client.Connected;
        public event Action<MessageData> MessageReceived;
        public event Action<string,string> UserNameChanged;
        public event Action OnLogin;
        private TcpClient _client = new TcpClient();
        private NetworkStream Stream => _client?.GetStream();

        public Dictionary<string, string> UserNameDictionary { get; private set; } = new Dictionary<string, string>();

        const char _separator = (char)3;


        public async Task Connect(string ip, int port)
        {
            _client?.Close();
            _client = new TcpClient();

            await _client.ConnectAsync(ip, port).ConfigureAwait(false);

            await ReadMessageLoop();
        }


        void ProcessMessage(MessageData msg)
        {
            switch (msg.Command)
            {
                case Command.Login:
                    OnUserLogin(msg);
                    break;
                case Command.ChangeName:
                    ProcessChangeName(msg);
                    break;
                case Command.Message:
                    MessageReceived?.Invoke(msg);
                    break;
                case Command.List:
                    OnUserListReceived(msg.Message);
                    break;
                default:
                    break;
            }

        }

        private void OnUserLogin(MessageData msg)
        {
            UserId = msg.Id;
            OnLogin?.Invoke();
        }

        public void ChangeUserNameRequest(string newName)
        {
            if (!IsConnected)
                return;

            var msg = new MessageData
            {
                Id = UserId,
                Message = newName,
                Command = Command.ChangeName,
                MessageTime = DateTime.Now,
            };
            Stream.WriteMessageAsync(msg);
        }

        private void OnUserListReceived(string userList)
        {
            UserNameDictionary = userList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(entry => entry.Split(_separator)).ToDictionary(entry => entry[0], entry => entry[1]);
        }

        private void ProcessChangeName(MessageData msg)
        {
            if (msg.Error)
            {
                MessageReceived?.Invoke(msg);
                return;
            }

            var entry = msg.Message.Split(_separator);
            var oldName = entry[0];
            var newName = entry[1];

            var id = UserNameDictionary.FirstOrDefault(user => user.Value == oldName).Key;
            if (id != null)
                UserNameDictionary[id] = newName;

            UserNameChanged?.Invoke(oldName, newName);
        }


        private async Task ReadMessageLoop()
        {
            while (_client.Connected)
            {
                var message = await Stream.ReadMessageAsync().ConfigureAwait(false);
                ProcessMessage(message);
            }
        }

        public void WriteMessageAsync(MessageData message)
        {
            Stream.WriteMessageAsync(message);
        }

        public void Close()
        {
            Stream.WriteMessageAsync(new MessageData {Command = Command.Logout, Id = UserId });
        }
    }
}
