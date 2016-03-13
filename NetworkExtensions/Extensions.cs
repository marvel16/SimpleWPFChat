using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CustomNetworkExtensions
{

    public enum Command
    {
        Message,
        Login,
        Logout,
        List,
        ChangeName
    }

    [Serializable]
    public class MessageData
    {
        /// <summary>
        /// 16 bytes message guid
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// client-server Commands
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// Chat message
        /// </summary>
        public string Message { get; set; } = String.Empty;
        /// <summary>
        /// User status (offline, online)
        /// </summary>
        public UserStatus Status { get; set; }
        /// <summary>
        /// Message sent time
        /// </summary>
        public DateTime MessageTime { get; set; }

        public enum UserStatus
        {
            Offline,
            Online,
        }

        public bool Error { get; set; }
    }

    public class User
    {
        private static int counter = 0;
        public string Name{ get; set; }
        public Guid Id { get; }
        public TcpClient Client { get; }
        public User(TcpClient client)
        {
            Name = $"User{counter++}";
            Id = Guid.NewGuid();
            Client = client;
        }
    }

    public static class TcpExtensions
    {
        public static byte[] ToByteArray(this object obj)
        {
            var formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.Serialize(stream, obj);

            var byteMessage = stream.ToArray();
            var bytes = new List<byte>(byteMessage.Length + sizeof(int));
            bytes.AddRange(BitConverter.GetBytes(byteMessage.Length));
            bytes.AddRange(byteMessage);

            stream.Close();

            return bytes.ToArray();
        }

        public static object ByteArrayToObject(this byte[] bytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();

                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);

                return binForm.Deserialize(memStream);
            }
        }

        public static async Task<byte[]> ReadFromStreamAsync(this NetworkStream stream, int length)
        {
            if(stream == null)
                throw new ArgumentException("stream");
            if(length < 1)
                throw new ArgumentException("Impossible to read 0 or less bytes");

            byte[] bytes = new byte[length];
            int readPos = 0;

            while (readPos < length)
                readPos += await stream.ReadAsync(bytes, readPos, bytes.Length - readPos);
            
            return bytes;
        }

        public static async Task<MessageData> ReadMessageAsync(this NetworkStream stream)
        {
            int headerLength = sizeof(int);
            byte[] header = await stream.ReadFromStreamAsync(headerLength);

            int messageLength = BitConverter.ToInt32(header, 0);
            byte[] message = await stream.ReadFromStreamAsync(messageLength);

            return message.ByteArrayToObject() as MessageData;
        }

        public static void WriteMessageAsync(this NetworkStream stream, MessageData msg)
        {
            if (stream == null)
                throw new ArgumentException(nameof(stream));

            byte[] bytes = msg.ToByteArray();
            stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }


}
