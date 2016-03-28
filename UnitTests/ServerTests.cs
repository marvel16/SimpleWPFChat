using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
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
                srv.UserDictionary.TryAdd(user.Id, user);
            }

            // act
            var entries = srv.ReturnUserList.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(entries.Length, count);
        }

        [Test]
        public void ChangeNameRequest_ValidNewName_ChangeSuccessful()
        {
            char sChar = Srv.Separator;
            MessageData response = null;
            var srv = new Mock<Srv> {CallBase = true};
            srv.Setup(o => o.WriteMessage(It.IsAny<MessageData>(), It.IsAny<TcpClient>())).
                Callback<MessageData, TcpClient>((message, client) =>  response = message);

            const int count = 3;
            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                srv.Object.UserDictionary.TryAdd(user.Id, user);
            }
            // arrange

            var msg = new MessageData
            {
                Id = srv.Object.UserDictionary.First(entry => entry.Value.Name == "User0").Key,
                Command = Command.ChangeName,
                Message = "newname",
            };

            // act
            srv.Object.ChangeNameRequest(msg);

            // assert
            Assert.AreEqual(Command.ChangeName, response.Command);
            Assert.AreEqual(false , response.Error);
            Assert.AreEqual("User0"+sChar+"newname" , response.Message);
        }

        [Test]
        public void ChangeNameRequest_NewNameExists_FailChange()
        {
            char sChar = Srv.Separator;
            MessageData response = null;
            var srv = new Mock<Srv> { CallBase = true };
            srv.Setup(o => o.WriteMessage(It.IsAny<MessageData>(), It.IsAny<TcpClient>())).
                Callback<MessageData, TcpClient>((message, client) => response = message);

            const int count = 3;
            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                srv.Object.UserDictionary.TryAdd(user.Id, user);
            }
            // arrange

            var msg = new MessageData
            {
                Id = srv.Object.UserDictionary.First(entry => entry.Value.Name == "User0").Key,
                Command = Command.ChangeName,
                Message = "User0",
            };

            // act
            srv.Object.ChangeNameRequest(msg);

            // assert
            Assert.AreEqual(Command.ChangeName, response.Command);
            Assert.AreEqual(true, response.Error);
            Assert.IsNotEmpty(response.Message);
        }

        [Test]
        public void ChangeNameRequest_EmptyNewName_FailChange()
        {
            char sChar = Srv.Separator;
            MessageData response = null;
            var srv = new Mock<Srv> { CallBase = true };
            srv.Setup(o => o.WriteMessage(It.IsAny<MessageData>(), It.IsAny<TcpClient>())).
                Callback<MessageData, TcpClient>((message, client) => response = message);

            const int count = 3;
            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                srv.Object.UserDictionary.TryAdd(user.Id, user);
            }
            // arrange

            var msg = new MessageData
            {
                Id = srv.Object.UserDictionary.First(entry => entry.Value.Name == "User0").Key,
                Command = Command.ChangeName,
                Message = string.Empty
            };

            // act
            srv.Object.ChangeNameRequest(msg);

            // assert
            Assert.AreEqual(Command.ChangeName, response.Command);
            Assert.AreEqual(true, response.Error);
            Assert.IsNotEmpty(response.Message);
        }

        [Test]
        public void BroadcastMessageFromClient_AllReceipientGetSenderId()
        {
            char sChar = Srv.Separator;
            const int count = 3;

            var response = new List<Guid>(count);
            var srv = new Mock<Srv> { CallBase = true };
            srv.Setup(o => o.WriteMessage(It.IsAny<MessageData>(), It.IsAny<TcpClient>())).
                Callback<MessageData, TcpClient>((message, client) => response.Add(message.Id));

            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                srv.Object.UserDictionary.TryAdd(user.Id, user);
            }

            // arrange
            var msg = new MessageData
            {
                Id = srv.Object.UserDictionary.First().Key,
            };

            // act
            srv.Object.BroadcastMessageFromClient(msg);

            // assert
            Assert.AreEqual(count - 1, response.Count);
            Assert.IsTrue(response.All(id => id == msg.Id));
        }

        [Test]
        public void BroadcastMessage_ReceipientsAllExceptSender()
        {
            char sChar = Srv.Separator;
            const int count = 3;

            var response = new List<Guid>(count);
            var srv = new Mock<Srv> { CallBase = true };
            srv.Setup(o => o.WriteMessage(It.IsAny<MessageData>(), It.IsAny<TcpClient>())).
                Callback<MessageData, TcpClient>((message, client) => response.Add(message.Id));

            for (int i = 0; i < count; i++)
            {
                var user = new User(new TcpClient());
                srv.Object.UserDictionary.TryAdd(user.Id, user);
            }

            // arrange
            var msg = new MessageData
            {
                Id = srv.Object.UserDictionary.First().Key,
            };

            // act
            srv.Object.BroadcastMessage(msg);

            // assert
            Assert.AreEqual(count, response.Count);
            Assert.IsTrue(response.All(id => id == msg.Id));
        }

    }
}
