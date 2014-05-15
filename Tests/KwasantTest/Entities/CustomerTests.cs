using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.CommunicationManager;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
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
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            customerRepo = new CustomerRepository(_uow);
            _fixture = new FixtureData(_uow);

            //initialize CommunicationManager and register for event
            CommunicationManager commManager = new CommunicationManager();
            commManager.SubscribeToAlerts();
        }

        [Test]
        [Category("Customer")]
        public void Customer_Add_CanCreateCustomer()
        {
            //SETUP
            //create a customer from fixture data
            CustomerDO curCustomerDO = _fixture.TestCustomer();

            //EXECUTE
            customerRepo.Add(curCustomerDO);
            customerRepo.UnitOfWork.SaveChanges();

            //VERIFY
            //check that it was saved to the db
            CustomerDO savedCustomerDO = customerRepo.GetByKey(curCustomerDO.CustomerID);
            Assert.AreEqual(curCustomerDO.FirstName,savedCustomerDO.FirstName);
            Assert.AreEqual(curCustomerDO.EmailAddress, savedCustomerDO.EmailAddress);


        }
    }
}
