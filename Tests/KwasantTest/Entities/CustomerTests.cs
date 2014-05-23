using System.Linq;
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
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _fixture = new FixtureData();

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
            _uow.UserRepository.Add(curUserDO);
            _uow.SaveChanges();

            //VERIFY
            //check that it was saved to the db
            UserDO savedUserDO = _uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == curUserDO.Id);
            Assert.AreEqual(curUserDO.PersonDO.FirstName, savedUserDO.PersonDO.FirstName);
            Assert.AreEqual(curUserDO.PersonDO.EmailAddress, savedUserDO.PersonDO.EmailAddress);


        }
    }
}
