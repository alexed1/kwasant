using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture, Ignore]
    public class EventTests 
    {
        public ICustomerRepository customerRepo;
        private IEventRepository eventRepo;
        private IEmailAddressRepository emailAddressRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;
        private Calendar _calendar;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _calendar = new Calendar(_uow);
            customerRepo = new CustomerRepository(_uow);
            eventRepo = new EventRepository(_uow);
            emailAddressRepo = new EmailAddressRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        //this is a core integration test: get the ics message through
        [Test]
        [Category("Invitation")]
        public void Event_Dispatch_CanSendICS()
        {
            EventRepository eventRepo = new EventRepository(_uow);
            AttendeeRepository attendeesRepo = new AttendeeRepository(_uow);
            List<AttendeeDO> attendees =
                new List<AttendeeDO>
                {
                    _fixture.TestAttendee1(),
                    _fixture.TestAttendee2()
                };
            attendees.ForEach(attendeesRepo.Add);

            EventDO eventDO = new EventDO
            {
                Description = "This is my test invitation",
                Summary = @"My test invitation",
                Location = @"Some place!",
                StartDate = DateTime.Today.AddMinutes(5),
                EndDate = DateTime.Today.AddMinutes(15),
                Attendees = attendees,
                Emails = new List<EmailDO>()
            };
            eventRepo.Add(eventDO);
            var curEvent = new Event();
            curEvent.Dispatch(eventDO);

            //Verify success
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
            EventDO curOriginalEventDO = _fixture.TestEvent1();

            //EXECUTE
            eventRepo.Add(curOriginalEventDO);
            eventRepo.UnitOfWork.SaveChanges();

            //VERIFY
            EventDO curRetrievedEventDO = eventRepo.GetByKey(curOriginalEventDO.EventID);
            Assert.AreEqual(curOriginalEventDO.EventID, curRetrievedEventDO.EventID);
        }

        [Test]
        [Category("Event")]
        public void Event_Add_FailsWhenAddEventWithTooShortDescription()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent1();
            curOriginalEventDO.Description = "X";

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
                {
                    eventRepo.Add(curOriginalEventDO);
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
            eventRepo.Add(curOriginalEventDO);
            eventRepo.UnitOfWork.SaveChanges();

            //VERIFY
            EventDO CurRetrievedEventDO = eventRepo.GetByKey(curOriginalEventDO.EventID);
            Assert.AreEqual(curOriginalEventDO.EventID, CurRetrievedEventDO.EventID);
        }

        [Test]
        [Category("Event")]
        public void Event_Add_FailAddEventIfPriorityValueZero()
        {
            //SETUP      
            EventDO curOriginalEventDO = new EventDO
            {
                StartDate = DateTime.Now.AddHours(3),
                EndDate = DateTime.Now.AddHours(2),
                Priority = 0,
                Sequence = 2,
                IsAllDay = true,
                Category = "Vacation",
                Class = "Public",
                Description = "This is a test event description.",
                Location = "Silicon Valley.",
                Status = "Closed",
                Summary = "This is a test event summary.",
                Transparency = "Transparent"
            };

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
            {
                eventRepo.Add(curOriginalEventDO);
            }
            );
        }

        [Test]
        [Category("Event")]
        public void Event_Add_FailAddEventIfSequenceValueZero()
        {
            //SETUP      
            EventDO curOriginalEventDO = new EventDO
            {
                StartDate = DateTime.Now.AddHours(3),
                EndDate = DateTime.Now.AddHours(2),
                Priority = 1,
                Sequence = 0,
                IsAllDay = true,
                Category = "Vacation",
                Class = "Public",
                Description = "This is a test event description.",
                Location = "Silicon Valley.",
                Status = "Closed",
                Summary = "This is a test event summary.",
                Transparency = "Transparent"
            };

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
            {
                eventRepo.Add(curOriginalEventDO);
            }
            );
        }

        [Test]
        [Category("Event")]
        public void Event_Add_FailAddEvenIfEndDateIsGreaterThanStartDate()
        {
            //SETUP      
            EventDO curOriginalEventDO = new EventDO
            {
                StartDate = DateTime.Now.AddHours(3),
                EndDate = DateTime.Now.AddHours(2),
                Priority = 2,
                Sequence = 2,
                IsAllDay = true,
                Category = "Vacation",
                Class = "Public",
                Description = "This is a test event description.",
                Location = "Silicon Valley.",
                Status = "Closed",
                Summary = "This is a test event summary.",
                Transparency = "Transparent"
            };

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
            {
                eventRepo.Add(curOriginalEventDO);
            }
            );
        }

        [Test]
        [Category("Event")]
        public void Event_Delete_CanDeleteEvent()
        {
            //SETUP      
            EventDO curOriginalEventDO = _fixture.TestEvent1();

            //EXECUTE
            eventRepo.Add(curOriginalEventDO);
            eventRepo.UnitOfWork.SaveChanges();

            EventDO curRetrievedEventDO = eventRepo.GetByKey(curOriginalEventDO.EventID);

            eventRepo.Remove(curRetrievedEventDO);
            eventRepo.UnitOfWork.SaveChanges();

            EventDO curDeletedEventDO = eventRepo.GetByKey(curRetrievedEventDO.EventID);

            //VERIFY            
            Assert.IsNull(curDeletedEventDO);
        }
    }
}
