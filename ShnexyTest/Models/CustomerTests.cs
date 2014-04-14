using System.Data.Entity;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using Data.Tools.Managers.CommunicationManager;
using NUnit.Framework;
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
            CommunicationManager commManager = new CommunicationManager();
            commManager.SubscribeToAlerts();
        }
        [Test]
        public void Customer_Add_CanCreateCustomer()
        {
            //create a customer from fixture data
            CustomerDO curCustomerDO = _fixture.TestCustomer();
            customerRepo.Add(curCustomerDO);
            customerRepo.UnitOfWork.SaveChanges();
            //check that it was saved to the db
            CustomerDO savedCustomerDO = customerRepo.GetByKey(curCustomerDO.CustomerID);
            Assert.AreEqual(curCustomerDO.FirstName,savedCustomerDO.FirstName);
            Assert.AreEqual(curCustomerDO.EmailAddress, savedCustomerDO.EmailAddress);

            //call Customer#Add
            //does event get called?



        }
    }
}
