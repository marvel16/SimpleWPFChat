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
            server.Start();

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
            
        }

        // Thread signal.
        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        // Accept one client connection asynchronously.
        private void BeginAcceptTcpClient()
        {
            while (true)
            {
                // Set the event to nonsignaled state.
                tcpClientConnected.Reset();

                // Start to listen for connections from a client.
                Console.WriteLine("Waiting for a connection...");

                // Accept the connection. 
                // BeginAcceptSocket() creates the accepted socket.
                _listener.BeginAcceptTcpClient(OnAcceptTcpClient, _listener);

                // Wait until a connection is made and processed before 
                // continuing.
                tcpClientConnected.WaitOne();
            }
        }

        // Process the client connection.
        private void OnAcceptTcpClient(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);

            _clients.TryAdd(new Guid(), client);
            Console.WriteLine("Client connected completed");

            ReadMessageAsync(client);

            // Signal the calling thread to continue.
            tcpClientConnected.Set();
        }

        public void Start()
        {
            _listener.Start();
            //Thread thread = new Thread(BeginAcceptTcpClient);
            //thread.Start();
            BeginAcceptTcpClient();
        }


        public async Task<MessageData> ReadMessageAsync(TcpClient client)
        {
            if (client == null)
                throw new Exception("ReadMessageAsync(): Client is null.");

            var stream = client.GetStream();

            MessageData msg = null;

            while (client.Connected)
            {
                int headerLength = sizeof(int);
                byte[] header = await stream.ReadMessageFromStreamAsync(headerLength);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] message = await stream.ReadMessageFromStreamAsync(messageLength);

                msg = message.ByteArrayToMessage();
                Console.WriteLine(msg.Message);
            }


            return msg;
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
