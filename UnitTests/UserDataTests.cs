﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SocketsChat_WPF;


namespace UnitTests
{
    [TestFixture]
    public class UserDataTests
    {
        [Test]
        public void PositiveTest()
        {
            Assert.That(7, Is.Positive);
        }

        [Test]
        public void TestMessageDataToByteConvertion()
        {
            MessageData expectedMessage = new MessageData
            {
                Id = Guid.Empty,
                CmdCommand = MessageData.Command.Login,
                Status = MessageData.UserStatus.Offline,
                Message = "Hello",
                UserName = "Marvel",
                MessageTime = DateTime.MinValue,
            };

            var convertedMessage = new MessageData(expectedMessage.ToBytes());
            Assert.That(expectedMessage.Message, Is.EqualTo(convertedMessage.Message));
            Assert.That(expectedMessage.UserName, Is.EqualTo(convertedMessage.UserName));
            Assert.That(expectedMessage.Id, Is.EqualTo(convertedMessage.Id));
            Assert.That(expectedMessage.Status, Is.EqualTo(convertedMessage.Status));
            Assert.That(expectedMessage.CmdCommand, Is.EqualTo(convertedMessage.CmdCommand));
        }

        [Test]
        public void TestMessageDataToByteConvertion2()
        {
            MessageData expectedMessage = new MessageData
            {
                Id = Guid.Empty,
                CmdCommand = MessageData.Command.Login,
                Status = MessageData.UserStatus.Offline,
                Message = null,
                UserName = "",
                MessageTime = DateTime.MinValue,
            };

            var convertedMessage = new MessageData(expectedMessage.ToBytes());
            Assert.That(expectedMessage.Message, Is.EqualTo(convertedMessage.Message));
            Assert.That(expectedMessage.UserName, Is.EqualTo(convertedMessage.UserName));
            Assert.That(expectedMessage.Id, Is.EqualTo(convertedMessage.Id));
            Assert.That(expectedMessage.Status, Is.EqualTo(convertedMessage.Status));
            Assert.That(expectedMessage.CmdCommand, Is.EqualTo(convertedMessage.CmdCommand));
        }

    }
}
