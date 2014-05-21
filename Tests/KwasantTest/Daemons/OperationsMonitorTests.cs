using System.Linq;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OperationsMonitorTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test]
        public void TestOperationManager()
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
            TrackingStatusRepository trackingStatusRepository = uow.TrackingStatusRepository;
            var bookingRequestDO = new BookingRequestDO();
            bookingRequestRepo.Add(bookingRequestDO);

            uow.SaveChanges();

            Assert.AreEqual(0, trackingStatusRepository.GetAll().Count());

            var om = new OperationsMonitor();
            DaemonTests.RunDaemonOnce(om);

            Assert.AreEqual(1, trackingStatusRepository.GetAll().Count());
        }
    }
}
