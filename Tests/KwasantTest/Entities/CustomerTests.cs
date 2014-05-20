using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.CommunicationManager;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Models
{
    [TestFixture]
    public class CustomerTests
    {
        public IUserRepository userRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            userRepo = new UserRepository(_uow);
            _fixture = new FixtureData(_uow);

            //initialize CommunicationManager and register for event
            CommunicationManager commManager = new CommunicationManager();
            commManager.SubscribeToAlerts();
        }

        [Test]
        [Category("Customer")]
        public void Customer_Add_CanCreateUser()
        {
            //SETUP
            //create a customer from fixture data
            UserDO curUserDO = _fixture.TestUser();

            //EXECUTE
            userRepo.Add(curUserDO);
            userRepo.UnitOfWork.SaveChanges();

            //VERIFY
            //check that it was saved to the db
            UserDO savedUserDO = userRepo.GetByKey(curUserDO.Id);
            Assert.AreEqual(curUserDO.FirstName,savedUserDO.FirstName);
            Assert.AreEqual(curUserDO.EmailAddress, savedUserDO.EmailAddress);


        }
    }
}
