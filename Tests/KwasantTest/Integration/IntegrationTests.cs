﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Daemons;
using Data.Constants;
using Data.Entities;
using Data.Entities.Constants;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Daemons;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using S22.Imap;
using StructureMap;
using Utilities;

namespace KwasantTest.Workflow
{
    [TestFixture]
    public class IntegrationTests
    {
        private IUnitOfWork _uow;
        private string _testUserEmail;
        private string _testUserEmailPassword;
        private string _archivePollEmail;
        private string _archivePollPassword;
        private Email _emailService;
        private OutboundEmail _outboundDaemon;
        private string _startPrefix;
        private string _endPrefix;
        private ImapClient _client;
        private FixtureData _fixture;

        public Stopwatch totalOperationDuration = new Stopwatch();
        public Stopwatch emailToRequestDuration = new Stopwatch();
        public Stopwatch requestToEmailDuration = new Stopwatch();
        public TimeSpan emailToRequestTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan requestToEmailTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan totalOperationTimeout = TimeSpan.FromSeconds(120);

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _testUserEmail = ConfigRepository.Get("OutboundUserName");
            _testUserEmailPassword = ConfigRepository.Get("OutboundUserPassword");

            _archivePollEmail = ConfigRepository.Get("ArchivePollEmailAddress");
            _archivePollPassword = ConfigRepository.Get("ArchivePollEmailPassword");
            _emailService= new Email(_uow);
           _outboundDaemon = new OutboundEmail();
           _startPrefix = "Start:";
           _endPrefix = "End:";
           _client = new ImapClient("imap.gmail.com", 993, _testUserEmail.Split('@')[0], _testUserEmailPassword, AuthMethod.Login, true);
           _fixture = new FixtureData();
        }

     

        //This is a core integration test that verifies that inbound email is being processed into BR's, and then an event created from
        //a BR is booked and dispatched into invitation email that is received
        [Test]
        [Category("IntegrationTests")]
        public void ITest_CanProcessBRCreateEventAndSendInvite()
        {
            //SETUP                     
            //setup start time and end time for test event. 
            var now = DateTimeOffset.Now;
            // iCal truncates time up to seconds so we need to truncate as well to be able to compare time
            var start = new DateTimeOffset(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, now.Offset).AddDays(1);
            var end = start.AddHours(1);

            EmailDO testEmail = CreateTestEmail(start, end);

            //EXECUTE
            SendEmailAndStartTimer(testEmail);
            BookingRequestDO foundBookingRequest = PollForBookingRequest(testEmail.Subject);
            EventDO testEvent = CreateTestEvent(foundBookingRequest);
            //start the stopwatch measuring time from email send
            requestToEmailDuration.Start();
            //run the outbound daemon to send any outgoing invite(s)
            DaemonTests.RunDaemonOnce(_outboundDaemon);
            PollInboxForEvent(start, end, testEmail.Subject);

            //VERIFY
            //check timeouts
            Assert.Less(emailToRequestDuration.Elapsed, emailToRequestTimeout, "Email to BookingRequest conversion timed out.");
            Assert.Less(requestToEmailDuration.Elapsed, requestToEmailTimeout, "BookingRequest to Invitation conversion timed out.");
            Assert.Less(totalOperationDuration.Elapsed, totalOperationTimeout, "Workflow timed out.");
        
        }


        //This method takes a NegotiationId as input that must resolve to a Negotiation with status "Unresolved".
        //Build a list of Users based on the Attendee lists of the associated Events.
        //For each user, if there is not already a ClarificationRequestDO that has this UserId and this NeogtiationId, call ClarificationRequest#Create, get back a ClarificationRequestDO, and associate it with the Negotiation (You will need to add a property of type NegotiationDO to ClarificationRequestDO).
        //call ClarificationRequest#GenerateResponseURL
        //call ClarificationRequest#Send
        //This will generate an email with a link that takes the recipient to a response view. 
        [Test]
        [Category("IntegrationTests")]
        public void ITest_CanDispatchNegotiationEmail()
        {
            //Create a BookingRequest and two test Attendees
            //Create a Negotiation with a Question and an Answer
            //Save the Negotiation as if running ProcessSubmittedNegotiation
            //Verify that Clarification Request was sent to Attendee
            //Verify that executing link in clarification request produces appropriate view

            BookingRequestDO testBR = _fixture.TestBookingRequest1();
            AttendeeDO testAttendee1 = _fixture.TestAttendee1();
            AttendeeDO testAttendee2 = _fixture.TestAttendee2();
            NegotiationDO testNegotiation = _fixture.TestNegotiation1();
            _uow.NegotiationsRepository.Add(testNegotiation);
            _uow.SaveChanges();


        }




        public void PollInboxForEvent(DateTimeOffset start, DateTimeOffset end, string subject)
        {
            //poll the specified account inbox until either the expected message is received, or timeout
            MailMessage inviteMessage = null;
            uint inviteMessageId = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                var uids = _client.Search(SearchCondition.Unseen()).ToList();

                //for each returned IMAP message id number...
                foreach (var uid in uids)
                {
                    //get the message...
                    var curMessage = _client.GetMessage(uid);
                    var icsView = curMessage.AlternateViews
                        .FirstOrDefault(v => string.Equals(v.ContentType.MediaType, "application/ics"));

                    //if it has an ICS attachment....
                    if (icsView != null)
                    {
                        var cal = iCalendar.LoadFromStream(icsView.ContentStream).FirstOrDefault();

                        //...and if that attachment has a cal with the expected event
                        if (cal != null && cal.Events.Count > 0 &&
                            cal.Events[0].Start.Value == start &&
                            cal.Events[0].End.Value == end)
                        {
                            //...then we're done.
                            inviteMessageId = uid;
                            inviteMessage = curMessage;
                            break;
                        }
                    }
                }
            } while (inviteMessage == null && requestToEmailDuration.Elapsed < requestToEmailTimeout);
            requestToEmailDuration.Stop();
            //cleanup the inbox by deleting the messages
            var requestMessages = _client.Search(SearchCondition.Subject(subject)).ToList();
            _client.DeleteMessages(requestMessages);
            if (inviteMessage != null)
            {
                _client.DeleteMessage(inviteMessageId);
            }
            totalOperationDuration.Stop();
        }

        public EventDO CreateTestEvent(BookingRequestDO testBR)
        {
            EventDO eventDO;
            //Programmatically create an event that matches (more or less) the provide booking request
            if (testBR != null)
            {
                var lines = testBR.HTMLText.Split(new[] {"\r\n"}, StringSplitOptions.None);
                var startString = lines[1].Remove(0, _startPrefix.Length);
                var endString = lines[2].Remove(0, _endPrefix.Length);
                var e = new Event();
                eventDO = e.Create(_uow, testBR.Id, startString, endString);
                eventDO.CreatedByID = "1";
                eventDO.Description = "test event description";
                _uow.EventRepository.Add(eventDO);
                e.Process(_uow, eventDO);
                _uow.SaveChanges();
                return eventDO;
            }
            return null;
        }
        public BookingRequestDO PollForBookingRequest(string subject)
        {
            BookingRequestDO foundBookingRequest;
            //run inbound daemon, checking for a generated BookingRequest, until success or timeout
            InboundEmail inboundDaemon = new InboundEmail();
            BookingRequestDO request;
            do
                {
                   DaemonTests.RunDaemonOnce(inboundDaemon);
                    foundBookingRequest = _uow.BookingRequestRepository.FindOne(
                           br => br.From.Address == _testUserEmail && br.Subject == subject);
                } while (foundBookingRequest == null && emailToRequestDuration.Elapsed < emailToRequestTimeout);
            emailToRequestDuration.Stop();
            return foundBookingRequest;
        }

        public void SendEmailAndStartTimer(EmailDO testEmail)
        {
            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            AddNewTestCustomer(testEmail.From);
            //EXECUTE
            _emailService.Send(testEmail);
            _uow.SaveChanges();
            DaemonTests.RunDaemonOnce(_outboundDaemon);
            totalOperationDuration.Start();
            emailToRequestDuration.Start();

        }

        public EmailDO CreateTestEmail(DateTimeOffset start, DateTimeOffset end)
        {
            //create a test email 
            var subject = string.Format("Event {0}", Guid.NewGuid());
          

            var body = string.Format("Event details:\r\n{0}{1}\r\n{2}{3}", _startPrefix, start, _endPrefix, end);
            var emailDO = new EmailDO()
            {
                From = Email.GenerateEmailAddress(_uow, new MailAddress(_testUserEmail)),
                Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("kwasantintegration@gmail.com")),
                        EmailParticipantTypeID = EmailParticipantType.To
                    }
                },
                Subject = subject,
                PlainText = body,
                HTMLText = body
            };
            _uow.EmailRepository.Add(emailDO);
            return emailDO;
        }



        //this should be DRYed up by using the newly modular functions, above.
        [Test, Ignore]
        [Category("Workflow")]
        public void Workflow_CanAddBcctoOutbound()
        {
            var requestToEmailTimeout = TimeSpan.FromSeconds(60);
            Stopwatch requestToEmailDuration = new Stopwatch();

            var subject = string.Format("Bcc Test {0}", Guid.NewGuid());
            var now = DateTimeOffset.Now;
            // iCal truncates time up to seconds so we need to truncate as well to be able to compare time
            var start = new DateTimeOffset(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, now.Offset).AddDays(1);
            var end = start.AddHours(1);
            var body = string.Format("Bcc Test details:\r\n{0}{1}\r\n{2}{3}", _startPrefix, start, _endPrefix, end);

            var emailDO = new EmailDO()
            {
                From = Email.GenerateEmailAddress(_uow, new MailAddress(_testUserEmail)),
                Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("kwasantintegration@gmail.com")),
                        EmailParticipantTypeID = EmailParticipantType.To
                    }
                },
                Subject = subject,
                PlainText = body,
                HTMLText = body,
                EmailStatusID = EmailStatus.Queued
            };

            _uow.EmailRepository.Add(emailDO);

            var envelope = new EnvelopeDO()
            {
                Email = emailDO,
                Handler = EnvelopeDO.GmailHander
            };
            _uow.EnvelopeRepository.Add(envelope);

            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            AddNewTestCustomer(emailDO.From);

            OutboundEmail outboundDaemon = new OutboundEmail();
            DaemonTests.RunDaemonOnce(outboundDaemon);
            ImapClient client = new ImapClient("imap.gmail.com", 993, _archivePollEmail, _archivePollPassword, AuthMethod.Login, true);
            _uow.SaveChanges();
            
            requestToEmailDuration.Start();

            int mailcount = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            var requestMessages = client.Search(SearchCondition.Subject(subject)).ToList();
                if (requestMessages.Count() > 0)
                {
            client.DeleteMessages(requestMessages);
                    mailcount = requestMessages.Count();
                    break;
                }
            } while (requestToEmailDuration.Elapsed < requestToEmailTimeout);
            requestToEmailDuration.Stop();

            Assert.AreEqual(1, mailcount);
        }

        private void AddNewTestCustomer(EmailAddressDO emailAddress)
        {
            var role = new Role();
            role.Add(_uow, new KwasantTest.Fixtures.FixtureData().TestRole());
            var u = new UserDO();
            var user = new User();
            UserDO currUserDO = new UserDO();
            currUserDO.EmailAddress = emailAddress;
            currUserDO.Calendars = new List<CalendarDO>() { 
                new CalendarDO()    {
                        Id=1,
                        Name="test"
                }
            };
            CalendarDO t = new CalendarDO();
            t.Name = "test";
            t.Id = 1;
            _uow.UserRepository.Add(currUserDO);
        }
    }
}