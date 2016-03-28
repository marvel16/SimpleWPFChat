using System;
using System.Collections.Generic;
using Client.Models;
using NetworkCommon.Entities;
using NUnit.Framework;

namespace UnitTests
{
    public class ClientTests
    {
        [Test]
        public void OnUserLogin_UserGuidRetrievedFromMessage()
        {
            // arrange
            var client = new ClientModel();

            var msg = new MessageData
            {
                Command = Command.Login,
                Id = Guid.NewGuid(),
            };
            
            Assert.AreEqual(Guid.Empty, client.UserId);
            //act
            client.ProcessMessage(msg);

            // assert
            Assert.AreEqual(msg.Id, client.UserId);

        }

        [Test]
        public void OnUserListReceived()
        {
            // arrange
            char separ = ClientModel.Separator;
            var client = new ClientModel();


            var list = $"{Guid.NewGuid()}{separ}User0,{Guid.NewGuid()}{separ}User1,{Guid.NewGuid()}{separ}User2,";
            var msg = new MessageData
            {
                Command = Command.List,
                Message = list
            };

            Assert.AreEqual(Guid.Empty, client.UserId);
            //act
            client.ProcessMessage(msg);

            // assert
            Assert.AreEqual(3 , client.UserNameDictionary.Count);
            Assert.IsTrue(client.UserNameDictionary.ContainsValue("User0"));
            Assert.IsTrue(client.UserNameDictionary.ContainsValue("User1"));
            Assert.IsTrue(client.UserNameDictionary.ContainsValue("User2"));
        }

        [Test]
        public void OnMessageReceived_ReceivedAndSendMessage_AreEqual()
        {
            // arrange
            var client = new ClientModel();

            var receivedMessage = new MessageData();

            client.MessageReceived += (message) => receivedMessage = message;

            var msg = new MessageData
            {
                Id = Guid.NewGuid(),
                Command = Command.Message,
                Message = "You've got message!",
                MessageTime = DateTime.Now,
                Error = false,
            };

            // act
            client.ProcessMessage(msg);

            // assert
            Assert.AreEqual(msg.Message, receivedMessage.Message);
            Assert.AreEqual(msg.Error, receivedMessage.Error);
            Assert.AreEqual(msg.MessageTime, receivedMessage.MessageTime);
            Assert.AreEqual(msg.Id, receivedMessage.Id);
            Assert.AreEqual(msg.Command, receivedMessage.Command);
        }

        [Test]
        public void ProcessChangeName_Error_NameRemainsUnchanged()
        {
            // arrange
            char separ = ClientModel.Separator;
            var client = new ClientModel();

            var guid = Guid.NewGuid();
            string userName = "User";
            var list = $"{guid}{separ}{userName}";

            // Set user Id
            client.ProcessMessage(new MessageData { Command = Command.Login, Id = guid });
            // Set user List
            client.ProcessMessage(new MessageData { Command = Command.List, Message = list });

            MessageData errorMsg = null;
            client.MessageReceived += (message) => errorMsg = message;

            var msg = new MessageData
            {
                Error = true,
                Id = Guid.NewGuid(),
                Command = Command.ChangeName,
                Message = "Error",
            };

            string newName = string.Empty;
            string oldName = string.Empty;
            client.UserNameChanged += (o, n) =>
            {
                oldName = o;
                newName = n;
            };

            // act
            client.ProcessMessage(msg);

            // assert
            Assert.IsNotNull(errorMsg);
            Assert.IsTrue(errorMsg.Error);
            Assert.AreEqual(msg.Message, errorMsg.Message);
            Assert.AreEqual(msg.Message, errorMsg.Message);
            Assert.AreEqual(userName, client.UserNameDictionary[client.UserId.ToString()]);
        }

        [Test]
        public void ProcessChangeName_NameChanged()
        {
            // arrange
            char separ = ClientModel.Separator;
            var client = new ClientModel();

            var guid = Guid.NewGuid();
            string userName = "User";
            var list = $"{guid}{separ}{userName}";

            // Set user Id
            client.ProcessMessage(new MessageData { Command = Command.Login, Id = guid });
            // Set user List
            client.ProcessMessage(new MessageData { Command = Command.List, Message = list });

            MessageData errorMsg = null;
            client.MessageReceived += (message) => errorMsg = message;

            var msg = new MessageData
            {
                Error = false,
                Id = Guid.NewGuid(),
                Command = Command.ChangeName,
                Message = userName + separ + "NewName",
            };

            string newName = string.Empty;
            string oldName = string.Empty;
            client.UserNameChanged += (o, n) =>
            {
                oldName = o;
                newName = n;
            };

            // act
            client.ProcessMessage(msg);

            // assert
            Assert.IsNull(errorMsg);
            Assert.AreEqual(newName, client.UserNameDictionary[client.UserId.ToString()]);
            Assert.AreNotEqual(oldName, client.UserNameDictionary[client.UserId.ToString()]);
        }
    }
}
