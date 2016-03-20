using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using NetworkCommon.Entities;

namespace NetworkCommon
{
    public static class TcpExtensions
    {
        public static byte[] ToByteArray(this object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.Serialize(stream, obj);

            byte[] byteMessage = stream.ToArray();
            List<byte> bytes = new List<byte>(byteMessage.Length + sizeof (int));
            bytes.AddRange(BitConverter.GetBytes(byteMessage.Length));
            bytes.AddRange(byteMessage);

            stream.Close();

            return bytes.ToArray();
        }

        public static object ByteArrayToObject(this byte[] bytes)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();

                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);

                return binForm.Deserialize(memStream);
            }
        }

        public static async Task<byte[]> ReadFromStreamAsync(this NetworkStream stream, int length)
        {
            if (stream == null)
                throw new ArgumentException("stream");
            if (length < 1)
                throw new ArgumentException("Impossible to read 0 or less bytes");

            byte[] bytes = new byte[length];
            int readPos = 0;

            while (readPos < length)
                readPos += await stream.ReadAsync(bytes, readPos, bytes.Length - readPos);

            return bytes;
        }

        public static async Task<MessageData> ReadMessageAsync(this NetworkStream stream)
        {
            int headerLength = sizeof (int);
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

        public static async Task WriteFileToNetStreamAsync(this NetworkStream netStream, FileStream fstream,
            IProgress<double> iProgress = null)
        {
            if (netStream == null)
                throw new ArgumentNullException(nameof(netStream));
            if (fstream == null)
                throw new ArgumentNullException(nameof(fstream));
            if (!(netStream.CanWrite && fstream.CanRead))
                throw new ArgumentException("Cannot read or write from/to underlying streams");

            const int buffLen = 8192;
            long fileSize = fstream.Length;
            byte[] headerBytes = BitConverter.GetBytes(fileSize);

            await netStream.WriteAsync(headerBytes, 0, headerBytes.Length);

            byte[] buffer = new byte[buffLen];

            int bytesRead;
            double totalRead = 0;
            while ((bytesRead = fstream.Read(buffer, 0, buffer.Length)) > 0)
            {
                await netStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
                iProgress?.Report(totalRead/fileSize);
            }
        }

        public static async Task ReadFileFromNetStreamAsync(this NetworkStream netStream, FileStream fstream,
            IProgress<double> iProgress = null)
        {
            if (netStream == null)
                throw new ArgumentNullException(nameof(netStream));
            if (fstream == null)
                throw new ArgumentNullException(nameof(fstream));
            if (!(fstream.CanWrite && netStream.CanRead))
                throw new ArgumentException("Cannot read or write from/to underlying streams");

            const int bufferSize = 8192;
            int headerLength = sizeof (long);
            byte[] header = await netStream.ReadFromStreamAsync(headerLength);
            long fileSize = BitConverter.ToInt64(header, 0);

            long bytesLeft = fileSize;
            int bytesToRead;
            while (bytesLeft > 0)
            {
                bytesToRead = bytesLeft < bufferSize ? (int) bytesLeft : bufferSize;

                byte[] buffer = await netStream.ReadFromStreamAsync(bytesToRead);
                fstream.Write(buffer, 0, buffer.Length);
                bytesLeft -= bytesToRead;
                iProgress?.Report(1.0 - bytesLeft/(double) fileSize);
            }
        }

        public static string BytesToString(this long bytes)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (bytes == 0)
                return "0" + suf[0];
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return num + suf[place];
        }
    }
}