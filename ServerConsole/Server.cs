using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomNetworkExtensions;


namespace ServerConsole
{
    class Server
    {
        static void Main(string[] args)
        {
            var server = new Srv();
            server.Start();

        }

    }

    class Srv
    {
        private readonly TcpListener _listener = new TcpListener(IPAddress.Any, 50000);
        private ConcurrentDictionary<Guid, User> _clients = new ConcurrentDictionary<Guid, User>();
        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);
        private object sync = new object();

        public Srv()
        {
            
        }


        public void Start()
        {
            _listener.Start();

            while (true)
            {
                tcpClientConnected.Reset();
                try
                {
                    AcceptTcpClientAsync();
                }
                catch (Exception e)
                {
                    WriteLine(e.ToString());
                }
                tcpClientConnected.WaitOne();

            }
        }

        private async Task AcceptTcpClientAsync()
        {
            WriteLine("Waiting for a connection...");

            TcpClient client = await _listener.AcceptTcpClientAsync();

            var user = new User(client);
            _clients.TryAdd(user.Id, user);
            WriteLine("New client connected...");

            var response = new MessageData
            {
                Id = user.Id,
                CmdCommand = MessageData.Command.Login,
                Message = user.Name,
                MessageTime = DateTime.Now
            };

            WriteMessage(response);

            tcpClientConnected.Set();
            ReadMessageAsync(client);
        }

        

        void ProcessMessage(MessageData msg)
        {
            switch (msg.CmdCommand)
            {
                case MessageData.Command.Login:
                    WriteMessage(msg);
                    break;
                case MessageData.Command.ChangeName:
                    break;
                case MessageData.Command.Message:
                    WriteLine($"{_clients[msg.Id].Name}: {msg.Message}");
                    Broadcast(msg);
                    break;
                case MessageData.Command.List:
                    ReturnUserList(msg);
                    break;
                case MessageData.Command.Logout:
                default:
                    RemoveClient(msg);
                    break;
            }

        }

        private void Broadcast(MessageData msg)
        {
            foreach (var client in _clients.Where(client => client.Key != msg.Id))
                WriteMessage(msg);
        }

        private void ChangeName(MessageData msg)
        {
            bool allowChange = _clients.All(client => client.Value.Name != msg.Message);
            var responce = new MessageData
            {
                Id = msg.Id,
                Message = msg.Message,
                MessageTime = DateTime.Now,
            };

            if (allowChange)
                WriteLine($"{_clients[msg.Id].Name}: Changed name to \"{msg.Message}\".");
            else
                WriteLine($"{_clients[msg.Id].Name}: Couldn't change name to \"{msg.Message}\" because user with that name already exists.");

            responce.Error = !allowChange;

            WriteMessage(responce);
        }

        private void ReturnUserList(MessageData msg)
        {
            Guid id = msg.Id;
            string usersList = string.Join(string.Empty, _clients.Where(u => u.Key != id).Select(user => $"{user.Value.Name},"));
            
            var responce = new MessageData
            {
                Id = id,
                Message = usersList,
                CmdCommand = MessageData.Command.List,
                MessageTime = DateTime.Now
            };

            WriteMessage(responce);
        }

        private void RemoveClient(MessageData msg)
        {
            User user;
            _clients.TryRemove(msg.Id, out user);
            user.Client.Close();
        }

        private void WriteLine(string line)
        {
            Console.WriteLine($"[{DateTime.Now}]: {line}");
        }

        private async Task<MessageData> ReadMessageAsync(TcpClient client)
        {
            if (client == null)
                throw new Exception("ReadMessageAsync(): Client is null.");

            var stream = client.GetStream();

            MessageData msg = null;

            while (client.Connected)
            {
                int headerLength = sizeof(int);
                byte[] header = await stream.ReadMessageFromStreamAsync(headerLength);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] message = await stream.ReadMessageFromStreamAsync(messageLength);

                msg = message.ByteArrayToMessage();
                ProcessMessage(msg);
            }

            return msg;
        }

        public async Task WriteMessage(MessageData msg)
        {
            var client = _clients[msg.Id].Client;
            byte[] bytes = msg.ToByteArray();
            var stream = client?.GetStream();
            stream?.WriteAsync(bytes, 0, bytes.Length);
        }

    }
}
