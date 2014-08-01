using System;
using Daemons;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class ThroughputMonitorTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            ConfigRepository.Set("ThroughputCheckingStartTime", DateTime.Now.Subtract(new TimeSpan(0, 1, 0, 0)).ToUniversalTime().ToString("hh:mm") + "z");
            ConfigRepository.Set("ThroughputCheckingEndTime", DateTime.Now.Add(new TimeSpan(0, 1, 0, 0)).ToUniversalTime().ToString("hh:mm") + "z");
        }

        [Test]
        public void TestThroughputManagerExpired()
        {
            var smsPackager = new Mock<ISMSPackager>();

            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(smsPackager.Object));

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
            
            var bookingRequestDO = new BookingRequestDO();
            bookingRequestDO.BookingRequestStateID = BookingRequestState.Unprocessed;
            bookingRequestDO.User = new FixtureData().TestUser();
            bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 1, 0, 0));
            bookingRequestRepo.Add(bookingRequestDO);

            uow.SaveChanges();

            var throughputMonitor = new ThroughputMonitor();
            DaemonTests.RunDaemonOnce(throughputMonitor);

            smsPackager.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), () => Times.Exactly(1));
        }


        [Test]
        public void TestThroughputManagerNotExpired()
        {
            ConfigRepository.Set("ThroughputCheckingStartTime", DateTime.Now.Add(new TimeSpan(0, 1, 0, 0)).ToString("hh:mm") + "z");

            var smsPackager = new Mock<ISMSPackager>();

            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(smsPackager.Object));

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

            var bookingRequestDO = new BookingRequestDO();
            bookingRequestDO.BookingRequestStateID = BookingRequestState.Unprocessed;
            bookingRequestDO.User = new FixtureData().TestUser();
            bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 0, 20, 0));
            bookingRequestRepo.Add(bookingRequestDO);

            uow.SaveChanges();

            var throughputMonitor = new ThroughputMonitor();
            DaemonTests.RunDaemonOnce(throughputMonitor);

            smsPackager.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), Times.Never);

        }

        [Test]
        public void TestThroughputManagerNotRunningOutsideSpecifiedTime()
        {
            var smsPackager = new Mock<ISMSPackager>();

            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(smsPackager.Object));

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

            var bookingRequestDO = new BookingRequestDO();
            bookingRequestDO.BookingRequestStateID = BookingRequestState.Unprocessed;
            bookingRequestDO.User = new FixtureData().TestUser();
            bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 0, 20, 0));
            bookingRequestRepo.Add(bookingRequestDO);

            uow.SaveChanges();

            var throughputMonitor = new ThroughputMonitor();
            DaemonTests.RunDaemonOnce(throughputMonitor);

            smsPackager.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
        }
    }
}
