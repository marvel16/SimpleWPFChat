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
        }

        public async Task<MessageData> ReadMessage()
        {
            byte[] messageLength = new byte[sizeof(int)];
            await Stream?.ReadAsync(messageLength, 0, messageLength.Length);
            int len = BitConverter.ToInt32(messageLength, 0);
            byte[] messageBytes = new byte[len];
            await Stream?.ReadAsync(messageBytes, 0, len);

            return (MessageData)messageBytes.ByteArrayToObject();
        }

        public async Task WriteMessage(MessageData message)
        {
            byte[] bytes = message.ToByteArray();
            Stream?.WriteAsync(bytes, 0, bytes.Length);
        }
        
    }
}
