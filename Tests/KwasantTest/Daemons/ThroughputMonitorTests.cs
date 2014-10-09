using System;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class ThroughputMonitorTests : BaseTest
    {
        private Func<DateTimeOffset> GetThroughputCheckingStartTime = () => DateTimeOffset.Now.AddHours(-1);
        private Func<DateTimeOffset> GetThroughputCheckingEndTime = () => DateTimeOffset.Now.AddHours(1);

        private Mock<ISMSPackager> _smsPackagerMock;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            
            _smsPackagerMock = new Mock<ISMSPackager>();
            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(_smsPackagerMock.Object));

            var configRepositoryMock = new Mock<IConfigRepository>();
            configRepositoryMock
                .Setup(c => c.Get<string>(It.IsAny<string>()))
                .Returns<string>(key =>
                                     {
                                         switch (key)
                                         {
                                             case "ThroughputCheckingStartTime":
                                                 return GetThroughputCheckingStartTime().ToString();
                                             case "ThroughputCheckingEndTime":
                                                 return GetThroughputCheckingEndTime().ToString();
                                             default:
                                                 return new MockedConfigRepository().Get<string>(key);
                                         }
                                     });
            var configRepository = configRepositoryMock.Object;
            ObjectFactory.Configure(cfg => cfg.For<IConfigRepository>().Use(configRepository));
        }

        [Test]
        public void TestThroughputManagerExpired()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                var bookingRequestDO = new BookingRequestDO();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.User = new FixtureData(uow).TestUser1();
                bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 1, 0, 0));
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                var throughputMonitor = new ThroughputMonitor();
                DaemonTests.RunDaemonOnce(throughputMonitor);

                _smsPackagerMock.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), () => Times.Exactly(1));
            }
        }


        [Test]
        public void TestThroughputManagerNotExpired()
        {
            GetThroughputCheckingStartTime = () => DateTimeOffset.Now.AddHours(1);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                var bookingRequestDO = new BookingRequestDO();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.User = new FixtureData(uow).TestUser1();
                bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 0, 20, 0));
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                var throughputMonitor = new ThroughputMonitor();
                DaemonTests.RunDaemonOnce(throughputMonitor);

                _smsPackagerMock.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
            }
        }

        [Test]
        public void TestThroughputManagerNotRunningOutsideSpecifiedTime()
        {
            var smsPackager = new Mock<ISMSPackager>();

            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(smsPackager.Object));

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                var bookingRequestDO = new BookingRequestDO();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.User = new FixtureData(uow).TestUser1();
                bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 0, 20, 0));
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                var throughputMonitor = new ThroughputMonitor();
                DaemonTests.RunDaemonOnce(throughputMonitor);

                smsPackager.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
            }
        }
    }
}
