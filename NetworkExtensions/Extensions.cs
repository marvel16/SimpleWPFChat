using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CustomNetworkExtensions
{


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
        public Command CmdCommand { get; set; }

        /// <summary>
        /// User name in chat
        /// </summary>
        public string UserName { get; set; } = String.Empty;
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

        public enum Command
        {
            //Log into the server
            Login,
            //Logout of the server
            Logout,
            //Send a text message to all the chat clients     
            Message,
            //Get a list of users in the chat room from the server
            List
        }

        //public MessageData() {}


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

        public static MessageData ByteArrayToMessage(this byte[] bytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            memStream.Close();

            return (MessageData)obj;
        }

        public static async Task<byte[]> ReadMessageFromStreamAsync(this NetworkStream stream, int messageLength)
        {
            if(stream == null)
                throw new Exception("Stream is null");
            if(messageLength < 1)
                throw new Exception("Impossible to read 0 or less bytes");

            byte[] bytes = new byte[messageLength];
            int readPos = 0;
            while (readPos < sizeof(int))
                readPos += await stream?.ReadAsync(bytes, readPos, bytes.Length - readPos);
            return bytes;
        }
    }


}
