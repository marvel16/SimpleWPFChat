using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketsChat_WPF
{
    

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

        public MessageData() {}

        public MessageData(byte[] data)
        {
            if (data == null || data.Length < 32)
                return;

            Id = new Guid(data.Take(16).ToArray());
            CmdCommand = (Command)BitConverter.ToInt32(data, 16);
            Status = (UserStatus)BitConverter.ToInt32(data, 20);
            int messageLength = BitConverter.ToInt16(data, 24);
            int userNameLength = BitConverter.ToInt16(data, 26);

            UserName = userNameLength > 0 ? Encoding.UTF8.GetString(data, 28, userNameLength) : String.Empty;
            int currentIdx = 28 + userNameLength;
            Message = messageLength > 0 ? Encoding.UTF8.GetString(data, currentIdx, messageLength) : String.Empty;
            currentIdx += messageLength;
            long messageTicks = BitConverter.ToInt64(data, currentIdx);
            MessageTime = DateTime.FromBinary(messageTicks);
        }

        public byte[] ToBytes()
        {
            var byteArray = new List<byte>();

            byteArray.AddRange(Id.ToByteArray());
            byteArray.AddRange(BitConverter.GetBytes((int)CmdCommand));
            byteArray.AddRange(BitConverter.GetBytes((int)Status));
            byteArray.AddRange(BitConverter.GetBytes((short)Message.Length));
            byteArray.AddRange(BitConverter.GetBytes((short)UserName.Length));
            byteArray.AddRange(Encoding.UTF8.GetBytes(UserName));
            byteArray.AddRange(Encoding.UTF8.GetBytes(Message));
            byteArray.AddRange(BitConverter.GetBytes(MessageTime.ToBinary()));

            return byteArray.ToArray();
        }

    }


    public class Client
    {
        Socket
    }

}
