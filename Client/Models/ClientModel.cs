using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Win32;
using NetworkCommon;
using NetworkCommon.Entities;

namespace Client.Models
{
    public class ClientModel
    {
        const char Separator = (char)3;
        public Guid UserId { get; private set; }
        public Dictionary<string, string> UserNameDictionary { get; private set; } = new Dictionary<string, string>();
        public bool Connected => _client.Connected;
        public event Action<MessageData> MessageReceived;
        public event Action<string,string> UserNameChanged;
        public event Action<string, string> DownloadFileRequest;
        public event Action OnLogin;

        private string filePathTransferFile;

        private TcpClient _client = new TcpClient();
        private NetworkStream Stream => _client?.GetStream();



        public async Task Connect(string ip, int port)
        {
            _client?.Close();
            _client = new TcpClient();

            await _client.ConnectAsync(ip, port).ConfigureAwait(false);

            await ReadMessageLoop();
        }

        public void ChangeUserNameRequest(string newName)
        {
            if (!Connected)
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

        public void FileTransferRequest(FileInfo fi)
        {
            if (!Connected)
                return;

            filePathTransferFile = fi.FullName;

            var msg = new MessageData
            {
                Id = UserId,
                Message = fi.Name + Separator + fi.Length,
                Command = Command.FileTransferRequest,
                MessageTime = DateTime.Now,
            };
            Stream.WriteMessageAsync(msg);
        }


        public void FileTransferResponce(bool acceptFile, string fileName, IProgress<double> iProgress = null)
        {
            var ip = _client.Client.LocalEndPoint as IPEndPoint;

            if (ip == null)
                return;

            int port = 50001;

            

            var listner = new TcpListener(IPAddress.Any, 50001);
            listner.Start();

            string filePath = string.Empty;

            var response = new MessageData
            {
                Id = UserId,
                Command = Command.FileTransferResponse,
                Error = !acceptFile,
                Message = acceptFile ? ip.Address.ToString() + Separator + port : string.Empty,
            };

            Stream.WriteMessageAsync(response);

            if (!acceptFile)
                return;

            var client = listner.AcceptTcpClient();
            var fStream = new FileStream(fileName, FileMode.Create);

            Task t = client.GetStream().ReadFileFromNetStreamAsync(fStream, iProgress);
        }

        public void WriteMessageAsync(MessageData message)
        {
            Stream.WriteMessageAsync(message);
        }

        public void Close()
        {
            Stream.WriteMessageAsync(new MessageData { Command = Command.Logout, Id = UserId });
        }

        private void ProcessMessage(MessageData msg)
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
                case Command.FileTransferRequest:
                    OnFileTransferRequestReceived(msg);
                    break;
                case Command.FileTransferResponse:
                    OnFileTransferResponse(msg);
                    break;
                default:
                    break;
            }

        }

        private void OnFileTransferRequestReceived(MessageData msg)
        {
            string[] fi = msg.Message.Split(Separator);
            var fileName = fi[0];
            var fileSize = fi[1];

            DownloadFileRequest?.Invoke(fileName, fileSize);
        }

        

        private async Task OnFileTransferResponse(MessageData msg)
        {
            if(msg.Error || string.IsNullOrEmpty(msg.Message))
                return;

            string[] info = msg.Message.Split(Separator);

            IProgress<double> iProgress = null; // TODO

            using (var file = new FileStream(filePathTransferFile, FileMode.Open))
            {
                var sendstream = new TcpClient(info[0], int.Parse(info[1]));
                await sendstream.GetStream().WriteFileToNetStreamAsync(file, iProgress);
                sendstream.Close();
            }

        }

        private void OnUserLogin(MessageData msg)
        {
            UserId = msg.Id;
            OnLogin?.Invoke();
        }

        private void OnUserListReceived(string userList)
        {
            UserNameDictionary = userList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                Select(entry => entry.Split(Separator)).
                ToDictionary(entry => entry[0], entry => entry[1]);
        }

        private void ProcessChangeName(MessageData msg)
        {
            if (msg.Error)
            {
                MessageReceived?.Invoke(msg);
                return;
            }

            var entry = msg.Message.Split(Separator);
            var oldName = entry[0];
            var newName = entry[1];

            var id = UserNameDictionary.FirstOrDefault(user => user.Value == oldName).Key;
            if (id != null)
                UserNameDictionary[id] = newName;

            UserNameChanged?.Invoke(oldName, newName);
        }


        private async Task ReadMessageLoop()
        {
            while (Connected)
            {
                var message = await Stream.ReadMessageAsync().ConfigureAwait(false);
                ProcessMessage(message);
            }
        }


    }
}
