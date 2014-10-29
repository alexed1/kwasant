using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Daemons;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Daemons;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Services
{
    [TestFixture]
    public class BookingRequestManagerTests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            var configRepositoryMock = new Mock<IConfigRepository>();
            configRepositoryMock
                .Setup(c => c.Get<string>(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    switch (key)
                    {
                        case "MaxBRIdle":
                            return "0.04";
                        case "EmailAddress_GeneralInfo":
                            return "info@kwasant.com";
                        default:
                            return new MockedConfigRepository().Get<string>(key);
                    }
                });
            var configRepository = configRepositoryMock.Object;
            ObjectFactory.Configure(cfg => cfg.For<IConfigRepository>().Use(configRepository));
        }

        private void AddTestRequestData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Bookit Services")) {};

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();
            }
        }


        [Test]
        [Category("BRM")]
        public void NewCustomerCreated()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AlertReporter curAnalyticsManager = new AlertReporter();
                curAnalyticsManager.SubscribeToAlerts();

                List<UserDO> customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(0, customersNow.Count);

                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                customersNow = uow.UserRepository.GetAll().ToList();

                Assert.AreEqual(1, customersNow.Count);
                Assert.AreEqual("customer@gmail.com", customersNow.First().EmailAddress.Address);
                Assert.AreEqual("customer@gmail.com", customersNow.First().UserName);
                Assert.AreEqual("Mister Customer", customersNow.First().FirstName);
                //test analytics system

                FactDO curAction = uow.FactRepository.FindOne(k => k.ObjectId == bookingRequest.Id);
                Assert.NotNull(curAction);
            }
        }

        [Test]
        [Category("BRM")]
        public void ExistingCustomerNotCreatedButUsed()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                List<UserDO> customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(0, customersNow.Count);

                UserDO user = fixture.TestUser1();
                uow.UserRepository.Add(user);
                uow.SaveChanges();

                customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(1, customersNow.Count);

                MailMessage message = new MailMessage(new MailAddress(user.EmailAddress.Address, user.FirstName),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(1, customersNow.Count);
                Assert.AreEqual(user.EmailAddress, customersNow.First().EmailAddress);
                Assert.AreEqual(user.FirstName, customersNow.First().FirstName);
            }
        }

        [Test]
        [Category("BRM")]
        public void ParseAllDay()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "CCADE",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.EventDuration.MarkAsAllDayEvent,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse30MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc30",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add30MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse60MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc60",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add60MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse90MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc90",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add90MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse120MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc120",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add120MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void ShowUnprocessedRequestTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                object requests = (new BookingRequest()).GetUnprocessed(uow);
                object requestNow =
                    uow.BookingRequestRepository.GetAll()
                        .Where(e => e.State == BookingRequestState.Unstarted)
                        .OrderByDescending(e => e.Id)
                        .Select(
                            e =>
                                new
                                {
                                    request = e,
                                    body =
                                        e.HTMLText.Trim().Length > 400
                                            ? e.HTMLText.Trim().Substring(0, 400)
                                            : e.HTMLText.Trim()
                                })
                        .ToList();

                Assert.AreEqual(requestNow, requests);
            }
        }

        [Test]
        [Category("BRM")]
        public void SetStatusTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Bookit Services")) {};

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);
                bookingRequest.State = BookingRequestState.Invalid;
                uow.SaveChanges();

                IEnumerable<BookingRequestDO> requestNow =
                    uow.BookingRequestRepository.GetAll().ToList().Where(e => e.State == BookingRequestState.Invalid);
                Assert.AreEqual(1, requestNow.Count());
            }
        }

        [Test]
        [Category("BRM")]
        public void GetBookingRequestsTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddTestRequestData();
                List<Object> requests = (new BookingRequest()).GetAllByUserId(uow.BookingRequestRepository, 0, 10, uow.BookingRequestRepository.GetAll().FirstOrDefault().User.Id);
                Assert.AreEqual(1, requests.Count);
            }
        }

        //This test takes too long see. KW-340. Temporarily ignoring it.
        [Test]
        [Category("BRM")]
        public void TimeOutStaleBRTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var timeOut = TimeSpan.FromSeconds(30);
                Stopwatch staleBRDuration = new Stopwatch();

                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Bookit Services")) {};
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                bookingRequest.State = BookingRequestState.Booking;
                bookingRequest.BookerID = bookingRequest.User.Id;
                bookingRequest.LastUpdated = DateTimeOffset.Now;
                
                uow.SaveChanges();

                staleBRDuration.Start();

                IEnumerable<BookingRequestDO> requestNow;
                do
                {
                    var om = new OperationsMonitor();
                    DaemonTests.RunDaemonOnce(om);
                    requestNow =
                        uow.BookingRequestRepository.GetAll()
                            .ToList()
                            .Where(e => e.State == BookingRequestState.Unstarted);

                } while (!requestNow.Any() || staleBRDuration.Elapsed > timeOut);
                staleBRDuration.Stop();

                uow.SaveChanges();

                requestNow = uow.BookingRequestRepository.GetAll().ToList().Where(e => e.State == BookingRequestState.Unstarted);
                Assert.AreEqual(1, requestNow.Count());
            }
        }
    }
}
