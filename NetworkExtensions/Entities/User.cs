using System;
using System.Net.Sockets;

namespace NetworkExtensions.Entities
{
    public class User
    {
        private static int counter;

        public User(TcpClient client)
        {
            Name = $"User{counter++}";
            Id = Guid.NewGuid();
            Client = client;
        }

        public string Name { get; set; }
        public Guid Id { get; }
        public TcpClient Client { get; }
    }
}