using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkCommon;
using NetworkCommon.Entities;


namespace ServerConsole
{
    class Server
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var server = new Srv();
            server.Start();
        }

    }

    public class Srv
    {
        private readonly TcpListener _listener = new TcpListener(IPAddress.Any, 50000);
        private object sync = new object();

        public static readonly char Separator = (char)3;
        public ConcurrentDictionary<Guid, User> UserDictionary { get; } = new ConcurrentDictionary<Guid, User>();

        public string ReturnUserList
        {
            get
            {
                return string.Join(string.Empty, UserDictionary.Select(user => $"{user.Value.Id}{Separator}{user.Value.Name},"));
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

        



        private void ProcessMessage(MessageData msg)
        {
            TryRemoveDisconnectedClients();

            if (!UserDictionary.Keys.Contains(msg.Id))
                return;

            switch (msg.Command)
            {
                case Command.ChangeName:
                    ChangeNameRequest(msg);
                    break;
                case Command.Message:
                    WriteLine($"{UserDictionary[msg.Id].Name}: {msg.Message}");
                    BroadcastMessageFromClient(msg);
                    break;
                case Command.List:
                    SendUserList(msg.Id, ReturnUserList);
                    break;
                case Command.FileTransferRequest:
                    ProcessFileRequest(msg);
                    break;
                case Command.FileTransferResponse:
                    ProcessFileTransferResponce(msg);
                    break;
                case Command.Logout:
                default:
                    RemoveClient(msg);
                    break;
            }

        }


        public void ChangeNameRequest(MessageData msg)
        {
            var oldName = UserDictionary[msg.Id].Name;
            var newName = msg.Message;

            string message = string.Empty;
            if (string.IsNullOrEmpty(msg.Message))
            {
                message = $"Can't change name to empty string or whitespace";
            }
            else if (UserDictionary.Any(client => client.Value.Name == msg.Message))
            {
                message = $"Couldn't change name to \"{newName}\" because user with that name already exists.";
            }

            var responce = new MessageData
            {
                Id = msg.Id,
                Command = Command.ChangeName,
                MessageTime = DateTime.Now,
            };

            if (!string.IsNullOrEmpty(message))
            {
                responce.Error = true;
                responce.Message = message;
                WriteLine($"{oldName}: {message}");
                WriteMessage(responce);
                return;
            }
            
            UserDictionary[msg.Id].Name = newName;
            responce.Message = oldName + Separator + newName;

            WriteLine($"User \"{oldName}\" changed name to \"{newName}\".");
            BroadcastMessage(responce);
        }

        

        public void ProcessFileRequest(MessageData msg)
        {
            var responce = new MessageData
            {
                Id = msg.Id,
                Command = Command.FileTransferRequest,
                Message = msg.Message,
            };
                
            BroadcastMessageFromClient(msg);
        }

        

        private void ProcessFileTransferResponce(MessageData msg)
        {
            BroadcastMessageFromClient(msg);
        }

        private void RemoveClient(MessageData msg)
        {
            User user;
            UserDictionary.TryRemove(msg.Id, out user);
            user.Client.Close();
        }

        private void TryRemoveDisconnectedClients()
        {
            var disconnectedUsers = UserDictionary.Where(u => !u.Value.Client.Connected).Select(c => c.Key);
            
            foreach (var userKey in disconnectedUsers)
            {
                User user;
                UserDictionary.TryRemove(userKey, out user);
            }
        }
        public void WriteLine(string line)
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

        public virtual void WriteMessage(MessageData msg , TcpClient tcpClient = null)
        {
            var client = tcpClient ?? UserDictionary[msg.Id].Client;
            client.GetStream().WriteMessageAsync(msg);
        }

        public void BroadcastMessageFromClient(MessageData msg)
        {
            foreach (var client in UserDictionary.Where(client => client.Key != msg.Id))
                WriteMessage(new MessageData
                {
                    Id = msg.Id,
                    Command = msg.Command,
                    Message = msg.Message,
                    MessageTime = DateTime.Now,
                },
                client.Value.Client);
        }

        public void BroadcastMessage(MessageData msg)
        {
            foreach (var client in UserDictionary)
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
            foreach (var userId in UserDictionary.Select(entry => entry.Value.Id))
                SendUserList(userId, userList);
        }

        public void SendUserList(Guid id, string list)
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

        private void AcceptTcpClient()
        {
            WriteLine("Waiting for a connection...");

            TcpClient client = _listener.AcceptTcpClient();

            var user = new User(client);
            UserDictionary.TryAdd(user.Id, user);
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

    }
}
