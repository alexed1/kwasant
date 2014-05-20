using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture, Ignore]
    public class CalendarTests
    {
        private IUnitOfWork _uow;
        private BookingRequestRepository _bookingRequestRepo;
        private FixtureData _fixture;
        
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _bookingRequestRepo = new BookingRequestRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        public void TestCalendarLoadsRelatedEvents()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EventRepository eventRepo = new EventRepository(_uow);
            EmailEmailAddressRepository emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();
            CustomerDO customerTwo = _fixture.TestCustomer2();
            
            customerRepo.Add(customerOne);
            customerRepo.Add(customerTwo);

            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                Id = 1,
                Customer = customerOne,
            };
            bookingRequestOne.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());
            BookingRequestDO bookingRequestTwo = new BookingRequestDO()
            {
                Id = 2,
                Customer = customerTwo,
            };
            bookingRequestTwo.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());

            _bookingRequestRepo.Add(bookingRequestOne);
            _bookingRequestRepo.Add(bookingRequestTwo);

            EventDO eventOne = new EventDO()
            {
                Id = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                BookingRequest = bookingRequestOne
            };

            EventDO eventTwo = new EventDO()
            {
                Id = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                BookingRequest = bookingRequestTwo
            };

            eventRepo.Add(eventOne);
            eventRepo.Add(eventTwo);

            _uow.SaveChanges();

            Calendar calendar = new Calendar(_uow);
            calendar.LoadBookingRequest(bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events.First().Id);
            Assert.AreEqual(1, events.First().BookingRequest.Id);
        }

        [Test]
        public void TestMoveAndReload()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EventRepository eventRepo = new EventRepository(_uow);
            EmailEmailAddressRepository emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();
            
            customerRepo.Add(customerOne);
            
            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                Id = 1,
                Customer = customerOne,
            };
            bookingRequestOne.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());


            _bookingRequestRepo.Add(bookingRequestOne);
            
            EventDO eventOne = new EventDO()
            {
                Id = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            };
            eventRepo.Add(eventOne);

            _uow.SaveChanges();

            Calendar calendar = new Calendar(_uow);
            calendar.LoadBookingRequest(bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(DateTime.Today, events.First().StartDate);

            DateTime newStart = DateTime.Today.AddDays(1);
            DateTime newEnd = newStart.AddHours(1);
            calendar.MoveEvent(1, newStart, newEnd);

            _uow.SaveChanges();
            calendar.Reload();

            events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(newStart, events.First().StartDate);
            Assert.AreEqual(newEnd, events.First().EndDate);
        }

        [Test]
        public void TestDelete()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EventRepository eventRepo = new EventRepository(_uow);
            EmailEmailAddressRepository emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                Id = 1,
                Customer = customerOne,
            };
            bookingRequestOne.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());

            _bookingRequestRepo.Add(bookingRequestOne);

            EventDO eventOne = new EventDO()
            {
                Id = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            };
            eventRepo.Add(eventOne);

            _uow.SaveChanges();

            Calendar calendar = new Calendar(_uow);
            calendar.LoadBookingRequest(bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(DateTime.Today, events.First().StartDate);

            calendar.DeleteEvent(1);

            _uow.SaveChanges();
            calendar.Reload();

            events = calendar.EventsList;
            Assert.AreEqual(0, events.Count);
        }

        [Test]
        public void TestAdd()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EmailEmailAddressRepository emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                Id = 1,
                Customer = customerOne,
            };
            bookingRequestOne.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());

            _bookingRequestRepo.Add(bookingRequestOne);

            _uow.SaveChanges();

            Calendar calendar = new Calendar(_uow);
            calendar.LoadBookingRequest(bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(0, events.Count);

            calendar.AddEvent(new EventDO
            {
                Id = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            });

            _uow.SaveChanges();
            calendar.Reload();

            events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
        }

        [Test]
        public void TestGet()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EventRepository eventRepo = new EventRepository(_uow);
            EmailEmailAddressRepository emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                Id = 1,
                Customer = customerOne,
            };
            bookingRequestOne.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());

            _bookingRequestRepo.Add(bookingRequestOne);

            EventDO eventOne = new EventDO()
            {
                Id = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            };
            eventRepo.Add(eventOne);

            _uow.SaveChanges();

            Calendar calendar = new Calendar(_uow);
            calendar.LoadBookingRequest(bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(DateTime.Today, events.First().StartDate);

            Assert.NotNull(calendar.GetEvent(1));
        }

        [Test]
        public void TestDispatch()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EmailEmailAddressRepository emailEmailAddressRepository = new EmailEmailAddressRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                Id = 1,
                Customer = customerOne,
            };
            bookingRequestOne.AddEmailParticipant(EmailParticipantType.FROM, emailEmailAddressRepository, new EmailAddressDO());

            _bookingRequestRepo.Add(bookingRequestOne);

            _uow.SaveChanges();

            List<EmailDO> emails = new EmailRepository(_uow).GetAll().ToList();
            Assert.AreEqual(0, emails.Count);

            Calendar calendar = new Calendar(_uow);
            calendar.LoadBookingRequest(bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(0, events.Count);

            calendar.AddEvent(new EventDO
            {
                Id = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            });

            var curEvent = new Event();
            curEvent.Dispatch(calendar.GetEvent(1));

            _uow.SaveChanges();

            emails = new EmailRepository(_uow).GetAll().ToList();
            Assert.AreEqual(1, emails.Count);
        }
    }
}
