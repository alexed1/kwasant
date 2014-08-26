using System;
using System.Collections.Generic;
using System.Linq;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Fixtures;
using KwasantTest.Utilities;
using NUnit.Framework;
using S22.Imap;
using StructureMap;
using Utilities;

namespace KwasantTest.Integration.BookingITests
{
    [TestFixture]
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
            var start = GenerateEventStartDate();
            var end = start.AddHours(1);
            EmailDO testEmail = CreateTestEmail(start, end, "Event");
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

            //EXECUTE
            BookingRequestDO foundBookingRequest = null;
            EmailDO eventEmail = null;
            using (_polling.NewTimer(_polling.totalOperationTimeout, "Workflow"))
            {
                _emailService.Send(testEmail);
                _uow.SaveChanges();

                //make sure queued outbound email gets sent.
                _polling.FlushOutboundEmailQueues();
                Console.WriteLine("Sending of test BR Complete");



                foundBookingRequest = PollForBookingRequest(testEmail, client, inboundDaemon);
                if (foundBookingRequest != null)
                {
                    Console.WriteLine("Booking Request detected in intake");
                    EventDO testEvent = CreateTestEvent(foundBookingRequest);
                    Console.WriteLine("Test Event Created");

                    //start the stopwatch measuring time from email send
                    using (_polling.NewTimer(_polling.requestToEmailTimeout, "BookingRequest to Invitation"))
                    {
                        //run the outbound daemon to send any outgoing invite(s)
                        _polling.FlushOutboundEmailQueues();
                        Console.WriteLine("Beginning Polling...");

                        eventEmail = PollMailboxForEvent(testEmail, client, start, end);
                    }
                }
            }

            //VERIFY
            Assert.NotNull(foundBookingRequest, "No BookingRequest found.");
            Assert.NotNull(eventEmail, "No Invitation found.");
            //check timeouts
            _polling.CheckTimeouts();
            client.Dispose();
        
        }

        public BookingRequestDO PollForBookingRequest(EmailDO targetCriteria, ImapClient client, InboundEmail inboundDaemon)
        {
            PollingEngine.InjectedEmailQuery injectedQuery = InjectedQuery_FindBookingRequest;

            List<EmailDO> queryResults = _polling.PollForEmail(injectedQuery, targetCriteria, "intake", client, inboundDaemon);
            BookingRequestDO foundBookingRequest = (BookingRequestDO)queryResults.FirstOrDefault();
            return foundBookingRequest;
        }


        public EmailDO PollMailboxForEvent(EmailDO targetCriteria, ImapClient client, DateTimeOffset start, DateTimeOffset end)
        {
            //poll the specified account inbox until either the expected message is received, or timeout
            var eventEmails = _polling.PollForEmail((criteria, unreadMessages) => InjectedQuery_FindSpecificEvent(unreadMessages, start, end), targetCriteria, "external", client);
            return eventEmails.FirstOrDefault();
        }
       


     

      

        #region Injected Queries
        //Injected Queries

        public static IEnumerable<EmailDO> InjectedQuery_FindEmailBySubject(EmailDO targetcriteria, List<EmailDO> unreadmessages)
        {
            return unreadmessages.Where(e => e.Subject == targetcriteria.Subject);
        }

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

        public static IEnumerable<EmailDO> InjectedQuery_FindSpecificEvent(List<EmailDO> unreadMessages, DateTimeOffset start, DateTimeOffset end)
        {
            return unreadMessages
                .Where(e => e.Attachments
                                .Where(a => a.Type == "application/ics")
                                .Select(a => iCalendar.LoadFromStream(a.GetData()).FirstOrDefault())
                                .Any(cal => cal != null && cal.Events.Count > 0 &&
                                            cal.Events[0].Start.Value == start &&
                                            cal.Events[0].End.Value == end));
        }

        public DateTimeOffset GenerateEventStartDate()
        {
            var now = DateTimeOffset.Now;
            // iCal truncates time up to seconds so we need to truncate as well to be able to compare time
            return new DateTimeOffset(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, now.Offset).AddDays(1);
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

        private EmailDO CreateTestEmail(DateTimeOffset start, DateTimeOffset end, string emailType)
        {
            var subject = string.Format("{0} {1}", emailType, Guid.NewGuid());
            var body = string.Concat(emailType, string.Format(" details:\r\n{0}{1}\r\n{2}{3}", _startPrefix, start, _endPrefix, end));

            EmailDO testEmail = _fixture.TestEmail3(); //integrationtesting@kwasant.net
            testEmail.Subject = subject;
            testEmail.HTMLText = body;
            testEmail.PlainText = body;

            return testEmail;
        }

        //this should be DRYed up by using the newly modular functions, above.
        [Test]
        [Category("Workflow")]
        public void ITest_CanAddBcctoOutbound()
        {
            var start = GenerateEventStartDate();
            var end = start.AddHours(1);

            _uow.UserRepository.Add(_fixture.TestUser3());
            
            var emailDO = CreateTestEmail(start, end, "Bcc Test");
            _uow.EmailRepository.Add(emailDO);
            var email = new Email(_uow);

            // EXECUTE
            email.Send(emailDO);
            _uow.SaveChanges();

            //THIS SHOULDN"T BE NECESSARY ANYMORE adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            //AddNewTestCustomer(emailDO.From);

            _polling.FlushOutboundEmailQueues();
            ImapClient client = new ImapClient("imap.gmail.com", 993, _archivePollEmail, _archivePollPassword,
                                               AuthMethod.Login, true);
            var emails = _polling.PollForEmail(InjectedQuery_FindEmailBySubject, emailDO, "external", client);
            Assert.AreEqual(1, emails.Count);
            _polling.CheckTimeouts();
        }
    }
}
