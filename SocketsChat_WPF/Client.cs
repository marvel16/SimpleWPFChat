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

        public event Action Connected;

        public async Task Connect(string ip, int port)
        {
            _Client?.Close();
            _Client = new TcpClient();

            await _Client.ConnectAsync(ip, port);

            Connected?.Invoke();
        }

        public async Task<MessageData> ReadMessageAsync()
        {
            int headerLength = sizeof (int);
            byte[] header = await Stream?.ReadMessageFromStreamAsync(headerLength);

            int messageLength = BitConverter.ToInt32(header, 0);
            byte[] message = await Stream?.ReadMessageFromStreamAsync(messageLength);

            return message.ByteArrayToMessage();
        }


        

        public async Task WriteMessageAsync(MessageData message)
        {
            byte[] bytes = message.ToByteArray();
            Stream?.WriteAsync(bytes, 0, bytes.Length);

        }

        
        
    }
}
