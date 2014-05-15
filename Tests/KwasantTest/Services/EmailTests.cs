using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;


namespace KwasantTest.Services
{
    [TestFixture]
    public class EmailTests
    {
        private IUnitOfWork _uow;
        private BookingRequestRepository _bookingRequestRepo;
        private FixtureData _fixture;
        private Email _curEmail;
        private EventDO _curEventDO;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            //_bookingRequestRepo = new BookingRequestRepository(_uow);
            _fixture = new FixtureData(_uow);
            _curEmail = new Email(_uow, new EmailDO());
            _curEventDO = new EventDO();
        }

        [Test]
        public void TestCalendarLoadsRelatedEvents()
        {
            CustomerRepository customerRepo = new CustomerRepository(_uow);
            EventRepository eventRepo = new EventRepository(_uow);

            CustomerDO customerOne = _fixture.TestCustomer();
            CustomerDO customerTwo = _fixture.TestCustomer2();
            
            customerRepo.Add(customerOne);
            customerRepo.Add(customerTwo);

            BookingRequestDO bookingRequestOne = new BookingRequestDO()
            {
                EmailID = 1,
                Customer = customerOne,
                From = new EmailAddressDO()
            };
            BookingRequestDO bookingRequestTwo = new BookingRequestDO()
            {
                EmailID = 2,
                Customer = customerTwo,
                From = new EmailAddressDO()
            };

            _bookingRequestRepo.Add(bookingRequestOne);
            _bookingRequestRepo.Add(bookingRequestTwo);

            EventDO eventOne = new EventDO()
            {
                EventID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                BookingRequest = bookingRequestOne
            };

            EventDO eventTwo = new EventDO()
            {
                EventID = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                BookingRequest = bookingRequestTwo
            };

            eventRepo.Add(eventOne);
            eventRepo.Add(eventTwo);

            _uow.SaveChanges();

            Calendar calendar = new Calendar(_uow, bookingRequestOne);
            List<EventDO> events = calendar.EventsList;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events.First().EventID);
            Assert.AreEqual(1, events.First().BookingRequest.EmailID);
        }


        [Test]
        [Category("Email")]
        public void CanCreateStandardInviteEmail()
        {
            //SETUP  
            _curEventDO = _fixture.TestEvent2();
            string expectedSubject = "Invitation via Kwasant: " + _curEventDO.Summary + "@ " + _curEventDO.StartDate;
           
            
            //EXECUTE
            EmailDO _curEmailDO = _curEmail.CreateStandardInviteEmail(_curEventDO);
            
            //VERIFY
            Assert.AreEqual(_curEmailDO.Subject,  expectedSubject);
            
        }

        [Test]
        [Category("Email")]
        public void CreateSIE_FailsIfInvalidEventInput()
        {
            //SETUP
            string expectedSubject = "Invitation via Kwasant: " + _curEventDO.Summary + "@ " + _curEventDO.StartDate;
            EmailDO _curEmailDO; 
            
            //EXECUTE
  
            //VERIFY
            Assert.Throws<ValidationException>(() =>
                {
                    _curEmailDO = _curEmail.CreateStandardInviteEmail(_curEventDO);
                }

            );

        }

   
       
    }
}
