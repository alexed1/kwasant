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
        private CommunicationManager _comm;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixture = new FixtureData();
            _event = new Event();
            _comm = new CommunicationManager();
            
        }

        //this is a core integration test: get the ics message through
        [Test]
        [Category("Invitation")]
        public void Event_Dispatch_CanSendICS()
        {
            EventRepository eventRepo = _uow.EventRepository;
            //AttendeeRepository attendeesRepo = _uow.AttendeeRepository;
            //List<AttendeeDO> attendees =
            //    new List<AttendeeDO>
            //    {
            //        _fixture.TestAttendee1(),
            //        _fixture.TestAttendee2()
            //    };
            //attendees.ForEach(attendeesRepo.Add);

            EventDO eventDO = _fixture.TestEvent4();
            eventRepo.Add(eventDO);
            var curEvent = new Event();
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curEvent.Process(uow, eventDO);
                uow.SaveChanges();
            }

            //Verify emails created in memory
            EmailDO resultEmail = eventDO.Emails[0];
            string expectedSubject = string.Format("Invitation from: " + _comm.GetOriginatorName(eventDO) + "- " + eventDO.Summary + " - " +eventDO.StartDate);
            Assert.AreEqual(expectedSubject, resultEmail.Subject);

            //Verify emails stored to disk properly
            EmailDO retrievedEmail = _uow.EmailRepository.GetQuery().First();
            Assert.AreEqual(expectedSubject, retrievedEmail.Subject);


            //use imap to load unread messages from the test customer account
            //verify that one of the messages is a proper ICS message
            //retry every 15 seconds for 1 minute



            //create an Email message addressed to the customer and attach the file.





            //skip for v.1: add EmailID to outbound queue



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

        [Test, Ignore]
        [Category("Event")]
        public void Event_Add_FailsWhenAddEventWithTooShortDescription()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent2();
            curOriginalEventDO.Description = "X";

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
                {
                    _uow.EventRepository.Add(curOriginalEventDO);
                }
            );

            //eventRepo.UnitOfWork.SaveChanges(); not necessary

            //VERIFY
            //handled in EXECUTE

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
