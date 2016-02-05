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
            



        }
        

    }

    class Srv
    {
        private readonly TcpListener _listener = new TcpListener(IPAddress.Any, 50000);

        private ConcurrentDictionary<Guid, TcpClient> _clients = new ConcurrentDictionary<Guid, TcpClient>();
        private ConcurrentQueue<MessageData> _messages = new ConcurrentQueue<MessageData>();
        private ManualResetEvent OnReadReceived = new ManualResetEvent(false);

        public Srv()
        {
            var listenThread = new Thread(DoBeginAcceptTcpClient);
            listenThread.Start();
        }

        // Thread signal.
        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        // Accept one client connection asynchronously.
        private void DoBeginAcceptTcpClient()
        {
            while (true)
            {
                // Set the event to nonsignaled state.
                tcpClientConnected.Reset();

                // Start to listen for connections from a client.
                Console.WriteLine("Waiting for a connection...");

                // Accept the connection. 
                // BeginAcceptSocket() creates the accepted socket.
                _listener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, _listener);

                // Wait until a connection is made and processed before 
                // continuing.
                tcpClientConnected.WaitOne();
            }
        }

        // Process the client connection.
        private async void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);

            _clients.TryAdd(new Guid(), client);
            Console.WriteLine("Client connected completed");

           await ReadMessage(client);

            // Signal the calling thread to continue.
            tcpClientConnected.Set();
        }

        public async void ServerLoop()
        {
            while (true)
            {
                ReadMessage(_clients.First().Value);
                OnReadReceived.WaitOne();
            }
        }

        private async Task ReadMessage(TcpClient client)
        {
            OnReadReceived.Reset();
            byte[] messageLength = new byte[sizeof(int)];
            try
            {
                var stream = client?.GetStream();

                await stream?.ReadAsync(messageLength, 0, messageLength.Length);

                int len = BitConverter.ToInt32(messageLength, 0);
                byte[] messageBytes = new byte[len];

                await stream?.ReadAsync(messageBytes, 0, len);

                _messages.Enqueue(messageBytes.ByteArrayToMessage());
                OnReadReceived.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnReceiveHeader(IAsyncResult ar)
        {
            try
            {
                TcpClient client = (TcpClient) ar.AsyncState;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task WriteMessage(TcpClient client, MessageData message)
        {
            byte[] bytes = message.ToByteArray();
            var stream = client?.GetStream();
            stream?.WriteAsync(bytes, 0, bytes.Length);

        }

    }
}
