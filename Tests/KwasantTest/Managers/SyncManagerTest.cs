using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.CalDAV;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Managers
{
    [TestFixture]
    public class SyncManagerTest
    {
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private SyncManager _syncManager;
        private UserDO _curUser;
        private readonly List<iCalendar> _remoteCalendarEvents = new List<iCalendar>();
        private RemoteCalendarProviderDO _curProvider;
        private RemoteCalendarAuthDataDO _curAuthData;

        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData();

            var clientMock = new Mock<ICalDAVClient>();
            clientMock.Setup(c =>
                             c.GetEventsAsync(
                                 It.IsAny<IRemoteCalendarLink>(),
                                 It.IsAny<DateTimeOffset>(),
                                 It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(_remoteCalendarEvents);
            clientMock.Setup(c =>
                              c.CreateEventAsync(It.IsAny<IRemoteCalendarLink>(),
                                                 It.IsAny<iCalendar>()))
                .Callback<IRemoteCalendarLink, iCalendar>(
                    (calendarLink, iCalEvent) => _remoteCalendarEvents.Add(iCalEvent));

            var clientFactoryMock = new Mock<ICalDAVClientFactory>();
            clientFactoryMock.Setup(f => f.Create(It.IsAny<IRemoteCalendarAuthData>())).Returns(clientMock.Object);

            _curUser = _fixtureData.TestUser();
            _curProvider = _fixtureData.TestRemoteCalendarProvider();
            _curAuthData = _fixtureData.TestRemoteCalendarAuthData(_curProvider, _curUser);
            _curUser.RemoteCalendarAuthData.Add(_curAuthData);

            _syncManager = new SyncManager(clientFactoryMock.Object);
        }

        private void AssertEventsAreEqual(EventDO expectedEvent, EventDO actualEvent)
        {
            Assert.AreEqual(expectedEvent.Summary, actualEvent.Summary, "Mock event and generated one are not equal (Summary).");
            Assert.AreEqual(expectedEvent.Description, actualEvent.Description, "Mock event and generated one are not equal (Description).");
            Assert.AreEqual(expectedEvent.StartDate, actualEvent.StartDate, "Mock event and generated one are not equal (StartDate).");
            Assert.AreEqual(expectedEvent.EndDate, actualEvent.EndDate, "Mock event and generated one are not equal (EndDate).");
            Assert.AreEqual(expectedEvent.Location, actualEvent.Location, "Mock event and generated one are not equal (Location).");
        }

        [Test]
        [Category("SyncManager")]
        public async void CanAddToLocalCalendar()
        {
            // SETUP
            var curEvent = _fixtureData.TestEvent2();
            var iCalEvent = Event.GenerateICSCalendarStructure(curEvent);
            _remoteCalendarEvents.Add(iCalEvent);

            // EXECUTE
            Assert.AreEqual(1, _remoteCalendarEvents.Count, "One event must be in the remote storage before synchronization.");
            Assert.AreEqual(0, _uow.EventRepository.GetAll().Count(), "No events must be in the repository before synchronization.");
            await _syncManager.SyncNowAsync(_uow, _curUser);

            // VERIFY
            var events = _uow.EventRepository.GetAll().ToArray();
            Assert.AreEqual(1, events.Length, "One event must be in the repository after synchronization.");
            var newEvent = events[0];
            AssertEventsAreEqual(curEvent, newEvent);
        }

        [Test]
        [Category("SyncManager")]
        public async void CanAddToRemoteCalendar()
        {
            // SETUP
            var curEvent = _fixtureData.TestEvent2();
            _uow.EventRepository.Add(curEvent);

            // EXECUTE
            Assert.AreEqual(1, _uow.EventRepository.GetAll().Count(), "One event must be in the repository before synchronization.");
            Assert.AreEqual(0, _remoteCalendarEvents.Count, "No events must be in the remote storage before synchronization.");
            await _syncManager.SyncNowAsync(_uow, _curUser);

            // VERIFY
            Assert.AreEqual(1, _remoteCalendarEvents.Count, "One event must be in the remote storage after synchronization.");
            var newEvent = Event.CreateEventFromICSCalendar(_remoteCalendarEvents[0]);
            AssertEventsAreEqual(curEvent, newEvent);
        }

        [Test]
        [Category("SyncManager")]
        public async void CanRemoveFromLocalCalendar()
        {
            // SETUP
            var curEvent = _fixtureData.TestEvent2();
            curEvent.DateCreated = DateTimeOffset.UtcNow - TimeSpan.FromDays(1);
            _uow.EventRepository.Add(curEvent);
            var curCalendarLink = _fixtureData.TestRemoteCalendarLink(_curProvider, _curUser);
            curCalendarLink.DateSynchronized = DateTimeOffset.UtcNow - TimeSpan.FromHours(1);
            _uow.RemoteCalendarLinkRepository.Add(curCalendarLink);

            // EXECUTE
            Assert.AreEqual(1, _uow.EventRepository.GetAll().Count(), "One event must be in the repository before synchronization.");
            Assert.AreEqual(0, _remoteCalendarEvents.Count, "No events must be in the remote storage before synchronization.");
            await _syncManager.SyncNowAsync(_uow, _curUser);

            // VERIFY
            Assert.AreEqual(0, _uow.EventRepository.GetAll().Count(), "No events must be in the repository after synchronization.");
            Assert.AreEqual(0, _remoteCalendarEvents.Count, "No events must be in the remote storage after synchronization.");
        }

        [Test]
        [Category("SyncManager")]
        public async void CanMergeCalendars()
        {
            // SETUP
            var curLocalEvent = _fixtureData.TestEvent2();
            _uow.EventRepository.Add(curLocalEvent);
            var curRemoteEvent = _fixtureData.TestEvent2();
            curRemoteEvent.Location = "changed location";
            curRemoteEvent.Description = "changed description";
            var iCalEvent = Event.GenerateICSCalendarStructure(curRemoteEvent);
            _remoteCalendarEvents.Add(iCalEvent);

            // EXECUTE
            Assert.AreEqual(1, _uow.EventRepository.GetAll().Count(), "One event must be in the repository before synchronization.");
            Assert.AreEqual(1, _remoteCalendarEvents.Count, "One event must be in the remote storage before synchronization.");
            await _syncManager.SyncNowAsync(_uow, _curUser);

            // VERIFY
            var localEvents = _uow.EventRepository.GetAll().ToArray();
            Assert.AreEqual(1, localEvents.Length, "One event must be in the repository after synchronization.");
            Assert.AreEqual(1, _remoteCalendarEvents.Count, "One event must be in the remote storage after synchronization.");
            var newLocalEvent = localEvents[0];
            var newRemoteEvent = Event.CreateEventFromICSCalendar(_remoteCalendarEvents[0]);
            AssertEventsAreEqual(newLocalEvent, newRemoteEvent);
            AssertEventsAreEqual(curRemoteEvent, newRemoteEvent);
        }
    }
}
