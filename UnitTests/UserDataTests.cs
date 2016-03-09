using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CustomNetworkExtensions;

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
                MessageTime = DateTime.MinValue,
            };


            byte[] bytes = expectedMessage.ToByteArray();
            var convertedMessage = bytes.ByteArrayToObject();

            Assert.That(expectedMessage.Message, Is.EqualTo(convertedMessage.Message));
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
                MessageTime = DateTime.MinValue,
            };


            byte[] bytes = expectedMessage.ToByteArray();
            var convertedMessage = bytes.ByteArrayToObject();

            Assert.That(expectedMessage.Message, Is.EqualTo(convertedMessage.Message));
            Assert.That(expectedMessage.Id, Is.EqualTo(convertedMessage.Id));
            Assert.That(expectedMessage.Status, Is.EqualTo(convertedMessage.Status));
            Assert.That(expectedMessage.CmdCommand, Is.EqualTo(convertedMessage.CmdCommand));
        }

    }
}
