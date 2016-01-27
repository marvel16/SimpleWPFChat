using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CustomNetworkExtensions;

namespace SocketsChat_WPF
{
    class Client
    {
        TcpClient _Client;
        public event Action<MessageData> MessageReceived;
    }
}
