using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public string ReturnUserList
        {
            get
            {
                return string.Join(string.Empty, _clients.Select(user => $"{user.Value.Id}={user.Value.Name},"));
            }
        }

        public Srv()
        {

        }


        public void Start()
        {
            try
            {
                _listener.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            while (true)
            {
                try
                {
                    AcceptTcpClient();
                }
                catch (Exception e)
                {
                    WriteLine(e.ToString());
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
            BroadcastUserListUpdate();

            Task t = MessageLoop(client);
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
                    BroadcastMessage(msg);
                    break;
                case MessageData.Command.List:
                    SendUserList(msg.Id, ReturnUserList);
                    break;
                case MessageData.Command.Logout:
                default:
                    RemoveClient(msg);
                    break;
            }

        }

        

        private void ChangeName(MessageData msg)
        {
            bool allowChange = _clients.All(client => client.Value.Name != msg.Message);
            var responce = new MessageData
            {
                Id = msg.Id,
                CmdCommand = MessageData.Command.ChangeName,
                Message = msg.Message,
                MessageTime = DateTime.Now,
            };

            if (allowChange)
            {
                _clients[msg.Id].Name = msg.Message;
                WriteLine($"{_clients[msg.Id].Name}: Changed name to \"{msg.Message}\".");
            }
            else
                WriteLine($"{_clients[msg.Id].Name}: Couldn't change name to \"{msg.Message}\" because user with that name already exists.");

            responce.Error = !allowChange;

            WriteMessage(responce);
            
            BroadcastUserListUpdate();
        }

        private void SendUserList(Guid id, string list)
        { 
            var response = new MessageData
                {
                    Id = id,
                    Message = list,
                    CmdCommand = MessageData.Command.List,
                    MessageTime = DateTime.Now,
                };

            WriteMessage(response);
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

        private async Task MessageLoop(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            while (client.Connected)
            {
                MessageData msg = await client.GetStream().ReadMessageAsync();
                ProcessMessage(msg);
            }
        }

        public void WriteMessage(MessageData msg , TcpClient tcpClient = null)
        {
            var client = tcpClient ?? _clients[msg.Id].Client;
            client.GetStream().WriteMessageAsync(msg);
        }

        private void BroadcastMessage(MessageData msg)
        {
            foreach (var client in _clients.Where(client => client.Key != msg.Id))
                WriteMessage(new MessageData()
                {
                    Id = msg.Id,
                    CmdCommand = MessageData.Command.Message,
                    Message = msg.Message,
                    MessageTime = DateTime.Now,
                }, client.Value.Client);
        }

        public void BroadcastUserListUpdate()
        {
            var userList = ReturnUserList;
            foreach (var userId in _clients.Select(entry => entry.Value.Id))
                SendUserList(userId, userList);
        }

    }
}
