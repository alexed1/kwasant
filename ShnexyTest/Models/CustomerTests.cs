using System;
using System.Data.Entity;
using NUnit.Framework;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.DataAccessLayer.StructureMap;
using Shnexy.Models;
using Shnexy.Services;
using ShnexyTest.Fixtures;
using StructureMap;

namespace ShnexyTest.Models
{
    [TestFixture]
    public class CustomerTests
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

            //initialize CommunicationManager and register for event
            var commManager = new CommunicationManager();
            commManager.SubscribeToAlerts();
        }
        [Test]
        public void Customer_Add_CanCreateCustomer()
        {
            //create a customer from fixture data
            Customer curCustomer = _fixture.TestCustomer();
            curCustomer.Add();
            customerRepo.UnitOfWork.SaveChanges();
            //check that it was saved to the db
            Customer savedCustomer = new Customer(customerRepo);
            savedCustomer = savedCustomer.GetByKey(curCustomer.Id);
            Assert.AreEqual(curCustomer.FirstName,savedCustomer.FirstName);
            Assert.AreEqual(curCustomer.emailAddr.Email, savedCustomer.emailAddr.Email);

            //call Customer#Add
            //does event get called?



        }
    }
}
