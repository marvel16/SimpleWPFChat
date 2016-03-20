using System;
using NetworkCommon;
using NetworkCommon.Entities;
using NUnit.Framework;

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
                Command = Command.Login,
                Message = "Hello",
                MessageTime = DateTime.MinValue,
            };


            byte[] bytes = expectedMessage.ToByteArray();
            var convertedMessage = (MessageData)bytes.ByteArrayToObject();

            Assert.That(expectedMessage.Message, Is.EqualTo(convertedMessage.Message));
            Assert.That(expectedMessage.Id, Is.EqualTo(convertedMessage.Id));
            Assert.That(expectedMessage.Status, Is.EqualTo(convertedMessage.Status));
            Assert.That(expectedMessage.Command, Is.EqualTo(convertedMessage.Command));
        }

        [Test]
        public void TestMessageDataToByteConvertion2()
        {
            MessageData expectedMessage = new MessageData
            {
                Id = Guid.Empty,
                Command = Command.Login,
                Message = null,
                MessageTime = DateTime.MinValue,
            };


            byte[] bytes = expectedMessage.ToByteArray();
            var convertedMessage = (MessageData)bytes.ByteArrayToObject();

            Assert.That(expectedMessage.Message, Is.EqualTo(convertedMessage.Message));
            Assert.That(expectedMessage.Id, Is.EqualTo(convertedMessage.Id));
            Assert.That(expectedMessage.Status, Is.EqualTo(convertedMessage.Status));
            Assert.That(expectedMessage.Command, Is.EqualTo(convertedMessage.Command));
        }

    }
}
