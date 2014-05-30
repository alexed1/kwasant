using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Daemons;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using Moq;
using NUnit.Framework;
using S22.Imap;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class InboundEmailTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test]
        public void TestInboundEmail()
        {
            var clientMock = new Mock<IImapClient>();

            const string testFromEmailAddress = "test.user@gmail.com";
            const string testSubject = "Test Subject";
            const string testBody = "Test Body";
            const string testToEmailAddress = "test.recipient@gmail.com";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(testFromEmailAddress),
                Subject = testSubject,
                Body = testBody,
            };
            
            mailMessage.To.Add(new MailAddress(testToEmailAddress));


            clientMock.Setup(c => c.Search(It.IsAny<SearchCondition>(), It.IsAny<String>())).Returns(new List<uint> { 1 });
            clientMock.Setup(c => c.GetMessage(1, true, null)).Returns(mailMessage);

            var ie = new InboundEmail(clientMock.Object);
            DaemonTests.RunDaemonOnce(ie);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var bookingRequestRepo = uow.BookingRequestRepository;
            var bookingRequests = bookingRequestRepo.GetAll().ToList();

            Assert.AreEqual(1, bookingRequests.Count);
            var bookingRequest = bookingRequests.First();
            Assert.AreEqual(testFromEmailAddress, bookingRequest.From.Address);
            Assert.AreEqual(testSubject, bookingRequest.Subject);
            Assert.AreEqual(testBody, bookingRequest.HTMLText);
            Assert.AreEqual(testFromEmailAddress, bookingRequest.User.EmailAddress.Address);
            Assert.AreEqual(1, bookingRequest.To.Count());
            Assert.AreEqual(testToEmailAddress, bookingRequest.To.First().Address);
        }
    }
}
