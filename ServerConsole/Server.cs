using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        private object sync = new object();

        public Srv()
        {
            
        }


        public void Start()
        {
            _listener.Start();

            while (true)
            {
                try
                {
                    AcceptTcpClient();
                }
                catch (Exception e)
                {
                    WriteLine(e.ToString());
                    Trace.WriteLine(e);
                }
            }
        }

        private void AcceptTcpClient()
        {
            WriteLine("Waiting for a connection...");

            TcpClient client = _listener.AcceptTcpClient();

            var user = new User(client);
            _clients.TryAdd(user.Id, user);
            WriteLine($"New client {user.Name} connected...");

            var response = new MessageData
            {
                Id = user.Id,
                CmdCommand = MessageData.Command.Login,
                Message = user.Name,
                MessageTime = DateTime.Now
            };

            WriteMessage(response);

            Task t = ReadMessageAsync(client);
        }

        

        void ProcessMessage(MessageData msg)
        {
            switch (msg.CmdCommand)
            {
                case MessageData.Command.Login:
                    WriteMessage(msg);
                    break;
                case MessageData.Command.ChangeName:
                    ChangeName(msg);
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
                WriteMessage(new MessageData()
                {
                    Id = client.Key,
                    CmdCommand = MessageData.Command.Message,
                    Message = msg.Message,
                    MessageTime = DateTime.Now,
                });
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
            string usersList = string.Join(string.Empty, _clients.Where(u => u.Key != id).Select(user => $"{user.Value.Id}={user.Value.Name},"));
            
            var responce = new MessageData
            {
                Id = id,
                Message = usersList,
                CmdCommand = MessageData.Command.List,
                MessageTime = DateTime.Now,
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

        private async Task ReadMessageAsync(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException();

            MessageData msg;

            while (client.Connected)
            {
                var stream = client.GetStream();
                int headerLength = sizeof(int);
                byte[] header = await stream.ReadMessageFromStreamAsync(headerLength);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] message = await stream.ReadMessageFromStreamAsync(messageLength);

                msg = message.ByteArrayToMessage();
                ProcessMessage(msg);
            }
        }

        public void WriteMessage(MessageData msg)
        {
            var client = _clients[msg.Id].Client;
            byte[] bytes = msg.ToByteArray();
            var stream = client?.GetStream();
            try
            {
                stream?.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
                Trace.WriteLine(e);
            }
        }

    }
}
