using System.Linq;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture]
    public class UserTests
    {
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _fixture = new FixtureData();
        }

        [Test, ExpectedException(ExpectedMessage = "Duplicate values for 'EmailAddressID' on 'UserDO' are not allowed. Duplicated value: '1'")]
        public void TestDuplicateUserEmailIDRejected()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserRepository.Add(new UserDO() { EmailAddressID = 1});
                uow.UserRepository.Add(new UserDO() { EmailAddressID = 1 });
                uow.SaveChanges();
            }
        }

        [Test]
        [Category("User")]
        public void CanAddUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var role = new Role();
                role.Add(uow, _fixture.TestRole());
                var u = new UserDO();
                var user = new User();

                //SETUP                 

                UserDO currUserDO = new UserDO();
                uow.UserRepository.Add(currUserDO);
               
                UserDO currRetrivedUserDO = uow.UserRepository.GetQuery().FirstOrDefault(uu => currUserDO.EmailAddressID == uu.EmailAddressID);
                
            }
        }


        [Test, Ignore]
        [Category("User")]
        public void CanAddUserSericeMethod()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                UserDO testUser = _fixture.TestUser1();
                uow.UserRepository.Add(testUser);
                uow.SaveChanges();
                var user = new User();
                user.Create(uow, testUser, "Admin", false);
                Assert.AreEqual(1, uow.UserRepository.GetAll().Count());

            }
        }

        [Test, Ignore]
        [Category("User")]
        public void CanUpdateUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                UserDO testUser = _fixture.TestUser2();
                uow.UserRepository.Add(testUser);
                uow.SaveChanges();
                testUser.FirstName = "Kwasant";
                var user = new User();
                user.Update(uow, testUser, "Admin");
                Assert.AreEqual(1, uow.UserRepository.GetAll().Count());

            }
        }
    }
}
