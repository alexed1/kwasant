﻿using System;
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
            StructureMapBootStrapper.ConfigureDependencies("test");
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

            var mailMessages = new List<MailMessage> { mailMessage };

            clientMock.Setup(c => c.Search(It.IsAny<SearchCondition>(), It.IsAny<String>())).Returns(new List<uint>());
            clientMock.Setup(c => c.GetMessages(It.IsAny<IEnumerable<uint>>(), true, null)).Returns(mailMessages);

            var ie = new InboundEmail(clientMock.Object);
            DaemonTests.RunDaemonOnce(ie);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var bookingRequestRepo = new BookingRequestRepository(uow);
            var bookingRequests = bookingRequestRepo.GetAll().ToList();

            Assert.AreEqual(1, bookingRequests.Count);
            var bookingRequest = bookingRequests.First();
            Assert.AreEqual(testFromEmailAddress, bookingRequest.From.Address);
            Assert.AreEqual(testSubject, bookingRequest.Subject);
            Assert.AreEqual(testBody, bookingRequest.Text);
            Assert.AreEqual(testFromEmailAddress, bookingRequest.Customer.EmailAddress);
            Assert.AreEqual(1, bookingRequest.To.Count);
            Assert.AreEqual(testToEmailAddress, bookingRequest.To.First().Address);
        }
    }
}