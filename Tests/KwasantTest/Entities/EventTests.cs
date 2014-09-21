using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture]
    public class EventTests 
    {
        public IUnitOfWork _uow;
        private FixtureData _fixture;
        private Event _event;
        private Invitation _invitation;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixture = new FixtureData();
            _event = ObjectFactory.GetInstance<Event>();
            _invitation = ObjectFactory.GetInstance<Invitation>();
            
        }

        //this is a core integration test: get the ics message through
        [Test]
        [Category("Invitation")]
        public void Event_Dispatch_CanSendICS()
        {
            EventRepository eventRepo = _uow.EventRepository;
            
            EventDO eventDO = _fixture.TestEvent4();
            eventDO.BookingRequest = _fixture.TestBookingRequest1();
           
            eventRepo.Add(eventDO);
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _event.InviteAttendees(uow, eventDO, eventDO.Attendees, new List<AttendeeDO>());
                uow.SaveChanges();
            }

            //Verify emails created in memory
            EmailDO resultEmail = eventDO.Emails[0];
            string expectedSubject = string.Format("Invitation from " + _invitation.GetOriginatorName(eventDO) + " -- " + eventDO.Summary + " - " + eventDO.StartDate.ToUniversalTime().ToString("ddd MMM dd, yyyy hh:mmtt - ") + eventDO.EndDate.ToUniversalTime().ToString("hh:mmtt") + " +00:00");
            Assert.AreEqual(expectedSubject, resultEmail.Subject);

            //Verify emails stored to disk properly
            EmailDO retrievedEmail = _uow.EmailRepository.GetQuery().First();
            Assert.AreEqual(expectedSubject, retrievedEmail.Subject);
        }

        [Test]
        [Category("Event")]
        public void Event_Add_CanAddEventWithRequiredFields()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent2();

            //EXECUTE
            _uow.EventRepository.Add(curOriginalEventDO);
            _uow.SaveChanges();

            //VERIFY
            EventDO curRetrievedEventDO = _uow.EventRepository.GetByKey(curOriginalEventDO.Id);
            Assert.AreEqual(curOriginalEventDO.Id, curRetrievedEventDO.Id);
        }

        [Test]
        [Category("Event")]
        public void Event_Add_CanAddEventWithAllFields()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent2();

            //EXECUTE
            _uow.EventRepository.Add(curOriginalEventDO);
            _uow.SaveChanges();

            //VERIFY
            EventDO CurRetrievedEventDO = _uow.EventRepository.GetByKey(curOriginalEventDO.Id);
            Assert.AreEqual(curOriginalEventDO.Id, CurRetrievedEventDO.Id);
        }

        [Test]
        [Category("Event")]
        public void Event_Add_FailAddEvenIfStartDateIsGreaterThanEndDate()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent2();
            curOriginalEventDO.EndDate = curOriginalEventDO.StartDate.AddHours(-1);

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
            {
                _uow.EventRepository.Add(curOriginalEventDO);
            }
            );
        }

        [Test]
        [Category("Event")]
        public void Event_Delete_CanDeleteEvent()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent1();
            curOriginalEventDO.CreatedBy = _fixture.TestUser1();
            curOriginalEventDO.Attendees = new List<AttendeeDO> { _fixture.TestAttendee1() };

            //EXECUTE
            _uow.EventRepository.Add(curOriginalEventDO);
            _uow.SaveChanges();

            EventDO curRetrievedEventDO = _uow.EventRepository.GetByKey(curOriginalEventDO.Id);

            _uow.EventRepository.Remove(curRetrievedEventDO);
            _uow.SaveChanges();

            EventDO curDeletedEventDO = _uow.EventRepository.GetByKey(curRetrievedEventDO.Id);

            //VERIFY            
            Assert.IsNull(curDeletedEventDO);
        }


        //CreatesOutboundEmails when an event is confirmed
        //setup:
        //create a sample event
        //dispatch it using event#dispatch
        //verify that an email has been created

        [Test]
        [Category("Event")]
        public void Event_CreateFailsIfNoCreatedBy()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent1();
            curOriginalEventDO.Attendees = new List<AttendeeDO> { _fixture.TestAttendee1() };
            curOriginalEventDO.CreatedByID = null;

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
            {
                _uow.EventRepository.Add(curOriginalEventDO);
                _uow.SaveChanges();

            });
            

        }

    }
}
