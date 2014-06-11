using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture]
    public class BookingRequestManagerTests
    {
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _fixture = new FixtureData();
        }

        [Test]
        [Category("BRM")]
        public void NewCustomerCreated()
        {
            List<UserDO> customersNow = _uow.UserRepository.GetAll().ToList();
            Assert.AreEqual(0, customersNow.Count);

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            customersNow = _uow.UserRepository.GetAll().ToList();
<<<<<<< HEAD

      
            Assert.AreEqual(1, customersNow.Count);
            Assert.AreEqual("customer@gmail.com", customersNow.First().EmailAddress.Address);
            Assert.AreEqual("Mister Customer", customersNow.First().FirstName);
            //test analytics system



            KactDO curAction = _uow.KactRepository.FindOne(k => k.ObjectId == bookingRequest.Id);
            Assert.NotNull(curAction);
=======
            Assert.AreEqual(2, customersNow.Count);
            Assert.AreEqual("customer@gmail.com", customersNow[0].EmailAddress.Address);
            Assert.AreEqual("Mister Customer", customersNow[0].EmailAddress.Name);
>>>>>>> dev
        }

        [Test]
        [Category("BRM")]
        public void ExistingCustomerNotCreatedButUsed()
        {
            List<UserDO> customersNow = _uow.UserRepository.GetAll().ToList();
            Assert.AreEqual(0, customersNow.Count);

            UserDO user = _fixture.TestUser();
            _uow.UserRepository.Add(user);
            _uow.SaveChanges();

            customersNow = _uow.UserRepository.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);

            MailMessage message = new MailMessage(new MailAddress(user.EmailAddress.Address, user.FirstName), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            customersNow = _uow.UserRepository.GetAll().ToList();
            Assert.AreEqual(2, customersNow.Count);
            Assert.AreEqual(user.EmailAddress, customersNow[0].EmailAddress);
            Assert.AreEqual(user.FirstName, customersNow[0].FirstName);
        }

        [Test]
        [Category("BRM")]
        public void ParseAllDay()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "CCADE",
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.EventDuration.MarkAsAllDayEvent, bookingRequest.Instructions.First().Id);
        }

        [Test]
        [Category("BRM")]
        public void Parse30MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc30",
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add30MinutesTravelTime, bookingRequest.Instructions.First().Id);
        }

        [Test]
        [Category("BRM")]
        public void Parse60MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc60",
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add60MinutesTravelTime, bookingRequest.Instructions.First().Id);
        }

        [Test]
        [Category("BRM")]
        public void Parse90MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc90",
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add90MinutesTravelTime, bookingRequest.Instructions.First().Id);
        }

        [Test]
        [Category("BRM")]
        public void Parse120MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Booqit Services"))
            {
                Body = "cc120",
            };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessNewBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add120MinutesTravelTime, bookingRequest.Instructions.First().Id);
        }
    }
}
