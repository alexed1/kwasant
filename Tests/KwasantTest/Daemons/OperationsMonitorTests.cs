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
            StructureMapBootStrapper.ConfigureDependencies("test");
        }

        [Test]
        public void TestInboundEmail()
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(uow);
            var bookingRequestDO = new BookingRequestDO();
            bookingRequestRepo.Add(bookingRequestDO);

            uow.SaveChanges();

            var om = new OperationsMonitor();
            DaemonTests.RunDaemonOnce(om);
        }
    }
}
