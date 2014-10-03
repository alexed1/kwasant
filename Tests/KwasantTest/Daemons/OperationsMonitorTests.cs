using System.Linq;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OperationsMonitorTests : BaseTest
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
            ObjectFactory.Configure(cfg => cfg.For<IConfigRepository>().Use(configRepositoryMock.Object));
        }

        [Test]
        public void TestOperationManager()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                TrackingStatusRepository trackingStatusRepository = uow.TrackingStatusRepository;
                var bookingRequestDO = new BookingRequestDO();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.User = new FixtureData(uow).TestUser1();
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                Assert.AreEqual(0, trackingStatusRepository.GetAll().Count());

                var om = new OperationsMonitor();
                DaemonTests.RunDaemonOnce(om);

                Assert.AreEqual(1, trackingStatusRepository.GetAll().Count());
            }
        }
    }
}
