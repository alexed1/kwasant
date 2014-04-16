using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using Data.Constants;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using Data.Tools.Managers;
using NUnit.Framework;
using ShnexyTest.Fixtures;
using StructureMap;

namespace ShnexyTest.Managers
{
    [TestFixture]
    public class BookingRequestManagerTests
    {
        public ICustomerRepository _customerRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<ShnexyDbContext>());
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow.Db.Database.Delete();
            _uow.Db.Database.Initialize(true);
            

            _customerRepo = new CustomerRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        [Category("BRM")]
        public void NewCustomerCreated()
        {
            List<CustomerDO> customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(0, customersNow.Count);

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);
            Assert.AreEqual("customer@gmail.com", customersNow.First().EmailAddress);
            Assert.AreEqual("Mister Customer", customersNow.First().FirstName);
        }

        [Test]
        [Category("BRM")]
        public void ExistingCustomerNotCreatedButUsed()
        {
            List<CustomerDO> customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(0, customersNow.Count);

            CustomerDO customer = _fixture.TestCustomer();
            _customerRepo.Add(customer);
            _uow.SaveChanges();

            customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);

            MailMessage message = new MailMessage(new MailAddress(customer.EmailAddress, customer.FirstName), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);
            Assert.AreEqual(customer.EmailAddress, customersNow.First().EmailAddress);
            Assert.AreEqual(customer.FirstName, customersNow.First().FirstName);
        }

        [Test]
        [Category("BRM")]
        public void ParseAllDay()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "CCADE",
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.EventDuration.MarkAsAllDayEvent, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BRM")]
        public void Parse30MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc30",
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add30MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BRM")]
        public void Parse60MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc60",
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add60MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BRM")]
        public void Parse90MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc90",
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add90MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BRM")]
        public void Parse120MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc120",
            };

            BookingRequestManager.ConvertEmail(_uow, message);

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add120MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }
    }
}
