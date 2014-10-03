using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.ExternalServices;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Daemons;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Integration.BookingITests
{
    [TestFixture]
    public class IntegrationTests : BaseTest
    {
        [Test]
        [Category("IntegrationTests")]
        public void ITest_CanProcessBRCreateEventAndSendInvite()
        {
            //The test is setup like this:
            //1. We sent an email which is to be turned into a booking request. This simulates a customer sending us an email
            //2. We confirm the booking request is created for the sent email
            //3. We create an event for that booking request
            //4. We call 'dispatch invitations' on the new event
            //5. We check that each attendee recieves an invitation

            //This lets us be sure we created the booking request for the right email
            string uniqueCustomerEmailSubject = Guid.NewGuid().ToString();
            
            //This stores the emails which we have sent, but not yet read. It lets us pass emails between the outbound and inbound mocks
            var unreadSentMails = new List<MailMessage>();
            var mockedImapClient = new Mock<IImapClient>();

            //When we are asked to fetch emails, return whatever we have in 'unreadSentEmails', then clear that list
            mockedImapClient.Setup(m => m.GetMessages(It.IsAny<IEnumerable<uint>>(),It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    var returnMails = new List<MailMessage>(unreadSentMails);
                    unreadSentMails.Clear();
                    return returnMails;
                });

            var mockedSmtpClient = new Mock<ISmtpClient>();
            //When we are asked to send an email, store it in unreadSentEmails
            mockedSmtpClient.Setup(m => m.Send(It.IsAny<MailMessage>())).Callback<MailMessage>(unreadSentMails.Add);

            ObjectFactory.Configure(o => o.For<IImapClient>().Use(mockedImapClient.Object));
            ObjectFactory.Configure(o => o.For<ISmtpClient>().Use(mockedSmtpClient.Object));

            //Create an email to be sent by the outbound email daemon
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Email email = ObjectFactory.GetInstance<Email>();
                var curEmail = email.GenerateBasicMessage(uow, uniqueCustomerEmailSubject, "Test bodyTest bodyTest bodyTest body", "me@gmail.com", "them@gmail.com");
                uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                uow.SaveChanges();
            }

            //Run outbound daemon to make sure we get 
            var outboundEmailDaemon = new OutboundEmail();
            DaemonTests.RunDaemonOnce(outboundEmailDaemon);

            //Now the booking request should be created
            var inboundEmailDaemon = new InboundEmail();
            DaemonTests.RunDaemonOnce(inboundEmailDaemon);

            //Now, find the booking request
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetQuery().FirstOrDefault(br => br.Subject == uniqueCustomerEmailSubject);
                Assert.NotNull(bookingRequestDO, "Booking request was not created.");

                //Create an event
                var e = ObjectFactory.GetInstance<Event>();
                var eventDO = e.Create(uow, bookingRequestDO.Id, DateTime.Now.ToString(), DateTime.Now.AddHours(1).ToString());
                uow.SaveChanges();
                
                //Dispatch invites for the event
                e.InviteAttendees(uow, eventDO, eventDO.Attendees, new List<AttendeeDO>());
                uow.SaveChanges();

                //Run our outbound email daemon so we can check if emails are created
                DaemonTests.RunDaemonOnce(outboundEmailDaemon);

                //Check each attendee recieves an invitation email
                foreach (var attendeeDO in eventDO.Attendees)
                {
                    Assert.True(unreadSentMails.Any(m => m.Subject.StartsWith("Invitation from me@gmail.com") && m.To.First().Address == attendeeDO.EmailAddress.Address), "Invitation not found for " + attendeeDO.Name);
                }
            }
        }
    }
}
