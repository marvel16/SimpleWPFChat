using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        const char _separator = (char)3;
        private object sync = new object();

        public string ReturnUserList
        {
            get
            {
                return string.Join(string.Empty, _clients.Select(user => $"{user.Value.Id}{_separator}{user.Value.Name},"));
            }
        }


        public void Start()
        {
            bool listnerStarted = false;
            do
            {
                try
                {
                    _listener.Start();
                    listnerStarted = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                }
            } while (!listnerStarted);

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
                Command = Command.Login,
                MessageTime = DateTime.Now
            };

            WriteMessage(response);
            TryRemoveDisconnectedClients();
            BroadcastUserListUpdate();

            Task t = MessageLoop(client);
        }



        void ProcessMessage(MessageData msg)
        {
            TryRemoveDisconnectedClients();
            switch (msg.Command)
            {
                case Command.ChangeName:
                    ChangeName(msg);
                    break;
                case Command.Message:
                    WriteLine($"{_clients[msg.Id].Name}: {msg.Message}");
                    BroadcastMessageFromClient(msg);
                    break;
                case Command.List:
                    SendUserList(msg.Id, ReturnUserList);
                    break;
                case Command.Logout:
                default:
                    RemoveClient(msg);
                    break;
            }

        }

        

        private void ChangeName(MessageData msg)
        {

            var responce = new MessageData
            {
                Id = msg.Id,
                Command = Command.ChangeName,
                MessageTime = DateTime.Now,
            };

            var oldName = _clients[msg.Id].Name;
            var newName = msg.Message;

            bool allowChange = !string.IsNullOrEmpty(msg.Message) && _clients.All(client => client.Value.Name != msg.Message);
            if (!allowChange)
            {
                string error = $"Couldn't change name to \"{newName}\" because user with that name already exists.";
                responce.Error = !allowChange;
                responce.Message = error;
                WriteLine($"{oldName}: {error}");
                WriteMessage(responce);
                return;
            }

            _clients[msg.Id].Name = newName;
            responce.Message = oldName + _separator + newName;
            WriteLine($"User \"{oldName}\" changed name to \"{newName}\".");
            BroadcastMessage(responce);
        }

        

        private void RemoveClient(MessageData msg)
        {
            User user;
            _clients.TryRemove(msg.Id, out user);
            user.Client.Close();
        }

        private void TryRemoveDisconnectedClients()
        {
            var clientsToRemove = _clients.Where(u => !u.Value.Client.Connected).Select(c => c.Key);

            foreach (var clientKey in clientsToRemove)
            {
                User user;
                _clients.TryRemove(clientKey, out user);
            }
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

        public void BroadcastMessageFromClient(MessageData msg)
        {
            foreach (var client in _clients.Where(client => client.Key != msg.Id))
                WriteMessage(new MessageData
                {
                    Id = msg.Id,
                    Command = Command.Message,
                    Message = msg.Message,
                    MessageTime = DateTime.Now,
                },
                client.Value.Client);
        }

        public void BroadcastMessage(MessageData msg)
        {
            foreach (var client in _clients)
                WriteMessage(new MessageData
                {
                    Id = msg.Id,
                    Command = msg.Command,
                    Message = msg.Message,
                    MessageTime = DateTime.Now,
                },
                client.Value.Client);
        }

        public void BroadcastUserListUpdate()
        {
            var userList = ReturnUserList;
            foreach (var userId in _clients.Select(entry => entry.Value.Id))
                SendUserList(userId, userList);
        }

        private void SendUserList(Guid id, string list)
        {
            var response = new MessageData
            {
                Id = id,
                Message = list,
                Command = Command.List,
                MessageTime = DateTime.Now,
            };

            WriteMessage(response);
        }

    }
}
