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
using StructureMap;
using Utilities;

namespace KwasantTest.Integration.BookingITests
{
    [TestFixture]
    public class IntegrationTests
    {
        private IUnitOfWork _uow;
        private IConfigRepository _configRepository;
        private string _outboundIMAPUsername;
        private string _outboundIMAPPassword;
        private string _archivePollEmail;
        private string _archivePollPassword;
        private Email _emailService;
  
        private string _startPrefix;
        private string _endPrefix;
        
        private FixtureData _fixture;
        
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _outboundIMAPUsername = _configRepository.Get("OutboundUserName");
            _outboundIMAPPassword = _configRepository.Get("OutboundUserPassword");

            _archivePollEmail = _configRepository.Get("ArchivePollEmailAddress");
            _archivePollPassword = _configRepository.Get("ArchivePollEmailPassword");
            _emailService = ObjectFactory.GetInstance<Email>();
          
           _startPrefix = "Start:";
           _endPrefix = "End:";
           _fixture = new FixtureData();
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
            //Programmatically create an event that matches (more or less) the provide booking request
            if (testBR != null)
            {
                var lines = testBR.HTMLText.Split(new[] {"\r\n"}, StringSplitOptions.None);
                var startString = lines[1].Remove(0, _startPrefix.Length);
                var endString = lines[2].Remove(0, _endPrefix.Length);
                var e = ObjectFactory.GetInstance<Event>();
                EventDO eventDO = e.Create(_uow, testBR.Id, startString, endString);
                eventDO.CreatedByID = "1";
                eventDO.Description = "test event description";
                _uow.EventRepository.Add(eventDO);
                e.InviteAttendees(_uow, eventDO, eventDO.Attendees, new List<AttendeeDO>());
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

    }
}
