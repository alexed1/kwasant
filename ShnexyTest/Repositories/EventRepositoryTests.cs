using System;
using System.Data.Entity;
using NUnit.Framework;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.DataAccessLayer.StructureMap;
using Shnexy.Models;
using ShnexyTest.Fixtures;
using StructureMap;

namespace ShnexyTest.Repositories
{
    [TestFixture]
    public class EventRepositoryTests
    {

        public ICustomerRepository customerRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {

            Database.SetInitializer(new DropCreateDatabaseAlways<ShnexyDbContext>());
            StructureMapBootStrapper.ConfigureDependencies("test");

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow.Db.Database.Initialize(true);

            customerRepo = new CustomerRepository(_uow);
            _fixture = new FixtureData(_uow);
        }


        [Test]
        public void EventRepository_CanAddEventRepository()
        {
            //SETUP
            //load a fixture event
            Event originalEvent = _fixture.TestEvent();

            //EXECUTE
            //call add method
            var curEventRepo = new EventRepository(_uow);
            curEventRepo.Add(originalEvent);
            curEventRepo.UnitOfWork.SaveChanges();

            //VERIFY
            Event retrievedEvent = curEventRepo.GetByKey(originalEvent.Id);
            //verify its the same
            Assert.AreEqual(originalEvent.Attendees, retrievedEvent.Attendees);
            Assert.AreEqual(originalEvent.DTStart, retrievedEvent.DTStart);
        }
    }
}
