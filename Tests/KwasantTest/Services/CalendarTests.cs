using System;
using System.Data.Entity;
using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using NUnit.Framework;
using ShnexyTest.Fixtures;
using StructureMap;

namespace ShnexyTest.Services
{
    [TestFixture]
    public class CalendarTests
    {
        private IUnitOfWork _uow;
        private BookingRequestRepository _bookingRequestRepo;
        private FixtureData _fixture;
        
        [SetUp]
        public void Setup()
        {
            Database.SetInitializer(new ShnexyInitializer());
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow.Db.Database.Delete();
            _uow.Db.Database.Initialize(true);


            _bookingRequestRepo = new BookingRequestRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        public void TestCalendarLoadsRelatedEvents()
        {
            var customerRepo = new CustomerRepository(_uow);
            var eventRepo = new EventRepository(_uow);

            var customerOne = _fixture.TestCustomer();
            var customerTwo = _fixture.TestCustomer();
            
            customerRepo.Add(customerOne);
            customerRepo.Add(customerTwo);

            var bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };
            var bookingRequestTwo = new BookingRequestDO()
            {
                EmailID = 2,
                Customer = customerTwo,
                From = new EmailAddressDO()
            };

            _bookingRequestRepo.Add(bookingRequestOne);
            _bookingRequestRepo.Add(bookingRequestTwo);

            var eventOne = new EventDO()
            {
                EventID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                BookingRequest = bookingRequestOne
            };

            var eventTwo = new EventDO()
            {
                EventID = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                BookingRequest = bookingRequestTwo
            };

            eventRepo.Add(eventOne);
            eventRepo.Add(eventTwo);

            _uow.SaveChanges();

            var calendar = new Calendar(_uow, bookingRequestOne);
            var events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events.First().EventID);
            Assert.AreEqual(1, events.First().BookingRequest.EmailID);
        }

        [Test]
        public void TestMoveAndReload()
        {
            var customerRepo = new CustomerRepository(_uow);
            var eventRepo = new EventRepository(_uow);

            var customerOne = _fixture.TestCustomer();
            
            customerRepo.Add(customerOne);
            
            var bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };
            
            _bookingRequestRepo.Add(bookingRequestOne);
            
            var eventOne = new EventDO()
            {
                EventID = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            };
            eventRepo.Add(eventOne);

            _uow.SaveChanges();

            var calendar = new Calendar(_uow, bookingRequestOne);
            var events = calendar.EventsList;
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
            var customerRepo = new CustomerRepository(_uow);
            var eventRepo = new EventRepository(_uow);

            var customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            var bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };

            _bookingRequestRepo.Add(bookingRequestOne);

            var eventOne = new EventDO()
            {
                EventID = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            };
            eventRepo.Add(eventOne);

            _uow.SaveChanges();

            var calendar = new Calendar(_uow, bookingRequestOne);
            var events = calendar.EventsList;
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
            var customerRepo = new CustomerRepository(_uow);

            var customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            var bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };

            _bookingRequestRepo.Add(bookingRequestOne);

            _uow.SaveChanges();

            var calendar = new Calendar(_uow, bookingRequestOne);
            var events = calendar.EventsList;
            Assert.AreEqual(0, events.Count);

            calendar.AddEvent(new EventDO
            {
                EventID = 1,
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
            var customerRepo = new CustomerRepository(_uow);
            var eventRepo = new EventRepository(_uow);

            var customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            var bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };

            _bookingRequestRepo.Add(bookingRequestOne);

            var eventOne = new EventDO()
            {
                EventID = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            };
            eventRepo.Add(eventOne);

            _uow.SaveChanges();

            var calendar = new Calendar(_uow, bookingRequestOne);
            var events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(DateTime.Today, events.First().StartDate);

            Assert.NotNull(calendar.GetEvent(1));
        }

        [Test]
        public void TestDispatch()
        {
            var customerRepo = new CustomerRepository(_uow);

            var customerOne = _fixture.TestCustomer();

            customerRepo.Add(customerOne);

            var bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };

            _bookingRequestRepo.Add(bookingRequestOne);

            _uow.SaveChanges();

            var emails = new EmailRepository(_uow).GetAll().ToList();
            Assert.AreEqual(1, emails.Count);

            var calendar = new Calendar(_uow, bookingRequestOne);
            var events = calendar.EventsList;
            Assert.AreEqual(0, events.Count);

            calendar.AddEvent(new EventDO
            {
                EventID = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BookingRequest = bookingRequestOne
            });

            calendar.DispatchEvent(calendar.GetEvent(1));

            _uow.SaveChanges();

            emails = new EmailRepository(_uow).GetAll().ToList();
            Assert.AreEqual(2, emails.Count);
        }
    }
}
