using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Daemons;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using FluentValidation.Validators;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Daemons;
using KwasantTest.Fixtures;
using KwasantTest.Utilities;
using Moq;
using NUnit.Framework;
using S22.Imap;
using StructureMap;
using Utilities;


namespace KwasantTest.Workflow
{
    [TestFixture, Ignore("Tests neveR finish!")]
    public class IntegrationTests
    {
        private IUnitOfWork _uow;
        private string _outboundIMAPUsername;
        private string _outboundIMAPPassword;
        private string _archivePollEmail;
        private string _archivePollPassword;
        private Email _emailService;
  
        private string _startPrefix;
        private string _endPrefix;
        
        private FixtureData _fixture;
        private PollingEngine _polling;

   
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _outboundIMAPUsername = ConfigRepository.Get("OutboundUserName");
            _outboundIMAPPassword = ConfigRepository.Get("OutboundUserPassword");

            _archivePollEmail = ConfigRepository.Get("ArchivePollEmailAddress");
            _archivePollPassword = ConfigRepository.Get("ArchivePollEmailPassword");
            _emailService= new Email(_uow);
          
           _startPrefix = "Start:";
           _endPrefix = "End:";
           _fixture = new FixtureData();
            _polling = new PollingEngine(_uow);
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
            var body  = CreateTestBody(start, end);

            EmailDO testEmail = _fixture.TestEmail3(); //integrationtesting@kwasant.net
            testEmail.HTMLText = body;
            testEmail.PlainText = body;
            string targetAddress = testEmail.To.First().Address;
            string targetPassword = "thorium65";
            ImapClient client = new ImapClient("imap.gmail.com", 993, targetAddress, targetPassword, AuthMethod.Login, true);
            InboundEmail inboundDaemon = new InboundEmail();
            inboundDaemon.username = targetAddress;
            inboundDaemon.password = targetPassword;

            //need to add user to pass OutboundEmail validation.
            _uow.UserRepository.Add(_fixture.TestUser3());
            _uow.EmailRepository.Add(testEmail);
            _uow.SaveChanges();
            Console.WriteLine("Setup Objects Complete");
            _polling.StartTimer();

            //EXECUTE
            _emailService.Send(testEmail);
            Console.WriteLine("Sending of test BR Complete");

            //make sure queued outbound email gets sent.
            _polling.FlushOutboundEmailQueues();
            


            BookingRequestDO foundBookingRequest = PollForBookingRequest(testEmail.Subject, client, inboundDaemon);
            Console.WriteLine("Booking Request detected in intake");
            EventDO testEvent = CreateTestEvent(foundBookingRequest);
            Console.WriteLine("Test Event Created");
            //start the stopwatch measuring time from email send
            _polling.requestToEmailDuration.Start();
            //run the outbound daemon to send any outgoing invite(s)
            _polling.FlushOutboundEmailQueues();
            Console.WriteLine("Beginning Polling...");

            PollMailboxForEvent(start, end, testEmail.Subject,client );

            //VERIFY
            //check timeouts
            _polling.CheckTimeouts();
            client.Dispose();
        
        }

        
     

        public BookingRequestDO PollForBookingRequest(string subject, ImapClient client, InboundEmail inboundDaemon)
        {
            EmailDO targetCriteria = _fixture.TestEmail3();
            PollingEngine.InjectedEmailQuery injectedQuery = InjectedQuery_FindBookingRequest;


            List<EmailDO> queryResults = _polling.PollForEmail(injectedQuery, targetCriteria, "intake", client, inboundDaemon);
            BookingRequestDO foundBookingRequest = (BookingRequestDO)queryResults.First();
            return foundBookingRequest;
        }

       


     

      

        #region Injected Queries
        //Injected Queries
      


        //This query looks for a single email of type booking request that meets provided From address and Subject criteria
        public static IEnumerable<EmailDO> InjectedQuery_FindBookingRequest(EmailDO targetCriteria, List<EmailDO> unreadMessages)
        {
            var UOW = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestDO foundBR = UOW.BookingRequestRepository.FindOne( br => br.From.Address == targetCriteria.From.Address && br.Subject == targetCriteria.Subject);
            return ConvertToEmailList(foundBR);
        }

        //================================================
        #endregion

        //to maximize reuse, all injected queries are converted to a List of EmailDO.
        public static List<EmailDO> ConvertToEmailList(object queryResults)
        {
             List<EmailDO> normalizedList= new List<EmailDO>();
             if (queryResults != null)
                 normalizedList.Add((EmailDO)queryResults);
             return normalizedList;
        }

        public static IEnumerable<EmailDO> InjectedQuery_FindSpecificEvent(EmailDO targetCriteria, List<EmailDO> unreadMessages)
        {
            var UOW = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestDO foundBR = UOW.BookingRequestRepository.FindOne(br => br.From.Address == targetCriteria.From.Address && br.Subject == targetCriteria.Subject);
            return ConvertToEmailList(foundBR);
        }

        public void PollMailboxForEvent(DateTimeOffset start, DateTimeOffset end, string subject, ImapClient client)
        {
            //poll the specified account inbox until either the expected message is received, or timeout
            MailMessage inviteMessage = null;
            uint inviteMessageId = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                
                var uids = client.Search(SearchCondition.Unseen()).ToList();
                //checkpoint. not finding any messages.  it loooks like this test submits imap mail and sends it to and tries to receive it at kwasantintegration@gmail.com. That could be getting shut down. switch
                //to submit using our normal submit: sendgrid, and send to integrationtesting@kwasant.net
                //move this stuff to integration testing setup


                //for each returned IMAP message id number...
                foreach (var uid in uids)
                {
                    //get the message...
                    var curMessage = client.GetMessage(uid);
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
                
            } while (inviteMessage == null && _polling.requestToEmailDuration.Elapsed < _polling.requestToEmailTimeout);
            _polling.requestToEmailDuration.Stop();
            //cleanup the inbox by deleting the messages
            var requestMessages = client.Search(SearchCondition.Subject(subject)).ToList();
            client.DeleteMessages(requestMessages);
            if (inviteMessage != null)
            {
                client.DeleteMessage(inviteMessageId);
            }
            _polling.totalOperationDuration.Stop();
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
     

     

        public string CreateTestBody(DateTimeOffset start, DateTimeOffset end)
        {
            //create a test email 
            var subject = string.Format("Event {0}", Guid.NewGuid());
            var body = string.Format("Event details:\r\n{0}{1}\r\n{2}{3}", _startPrefix, start, _endPrefix, end);
            
            return body;
        }



        //this should be DRYed up by using the newly modular functions, above.
        [Test, Ignore]
        [Category("Workflow")]
        public void Workflow_CanAddBcctoOutbound()
        {
           // var _polling.requestToEmailTimeout = TimeSpan.FromSeconds(60);
           // Stopwatch _polling.requestToEmailDuration = new Stopwatch();

            var subject = string.Format("Bcc Test {0}", Guid.NewGuid());
            var now = DateTimeOffset.Now;
            // iCal truncates time up to seconds so we need to truncate as well to be able to compare time
            var start = new DateTimeOffset(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, now.Offset).AddDays(1);
            var end = start.AddHours(1);
            var body = string.Format("Bcc Test details:\r\n{0}{1}\r\n{2}{3}", _startPrefix, start, _endPrefix, end);

            var emailDO = new EmailDO()
            {
                From = Email.GenerateEmailAddress(_uow, new MailAddress(_outboundIMAPUsername)),
                Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("kwasantintegration@gmail.com")),
                        EmailParticipantType = EmailParticipantType.To
                    }
                },
                Subject = subject,
                PlainText = body,
                HTMLText = body,
                EmailStatus = EmailState.Queued
            };

            _uow.EmailRepository.Add(emailDO);

            var envelope = new EnvelopeDO()
            {
                Email = emailDO,
                Handler = EnvelopeDO.GmailHander
            };
            _uow.EnvelopeRepository.Add(envelope);

            //THIS SHOULDN"T BE NECESSARY ANYMORE adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            //AddNewTestCustomer(emailDO.From);

            OutboundEmail outboundDaemon = new OutboundEmail();
            DaemonTests.RunDaemonOnce(outboundDaemon);
            ImapClient client = new ImapClient("imap.gmail.com", 993, _archivePollEmail, _archivePollPassword, AuthMethod.Login, true);
            _uow.SaveChanges();
            
            _polling.requestToEmailDuration.Start();

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
            } while (_polling.requestToEmailDuration.Elapsed < _polling.requestToEmailTimeout);
            _polling.requestToEmailDuration.Stop();

            Assert.AreEqual(1, mailcount);
        }

     
    }
}
