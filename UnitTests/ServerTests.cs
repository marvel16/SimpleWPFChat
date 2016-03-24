using System;
using System.Linq;
using System.Net.Sockets;
using Moq;
using NetworkCommon.Entities;
using NUnit.Framework;
using ServerConsole;

namespace UnitTests
{
    public class ServerTests
    {

        [Test]
        public void UserDictionary_NamesAreUnique()
        {
            // arrange
            var srv = new Srv();
            const int count = 10000;
            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                // act
                srv.UserDictionary.TryAdd(user.Id, user);
            }

            Assert.AreEqual(srv.UserDictionary.Select(pair => pair.Value).Distinct().Count(), count);
        }

        [Test]
        public void UserList_ReturnsSeparatedValues()
        {
            char sChar = Srv.Separator;

            // arrange
            var srv = new Srv();
            const int count = 3;
            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                // act
                srv.UserDictionary.TryAdd(user.Id, user);
            }

            var entries = srv.ReturnUserList.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(entries.Length, count);
        }
    }
}
