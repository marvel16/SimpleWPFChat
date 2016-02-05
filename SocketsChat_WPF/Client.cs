using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CustomNetworkExtensions;

namespace SocketsChat_WPF
{
    class Client
    {
        private TcpClient _Client;

        private NetworkStream Stream => _Client?.GetStream();
        public event Action<MessageData> MessageReceived;
        public event Action<Client> Connected;

        public async Task Connect(string ip, int port)
        {
            _Client?.Close();
            _Client = new TcpClient();

            await _Client.ConnectAsync(ip, port);

            Connected?.Invoke(this);
        }

        public async Task<MessageData> ReadMessageAsync()
        {
            if (Stream == null)
                throw new Exception("ReadMessageAsync(): Stream is null.");
            MessageData msg = null;
            while (_Client.Connected)
            {
                int headerLength = sizeof(int);
                byte[] header = await Stream.ReadMessageFromStreamAsync(headerLength);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] message = await Stream.ReadMessageFromStreamAsync(messageLength);

                msg = message.ByteArrayToMessage();
                MessageReceived?.Invoke(msg);
            }
            

            return msg;
        }


        

        public async Task WriteMessageAsync(MessageData message)
        {
            if(Stream == null)
                throw new Exception("WriteMessageAsync() : Stream is null");
            byte[] bytes = message.ToByteArray();
            Stream.WriteAsync(bytes, 0, bytes.Length);

        }

        
        
    }
}
