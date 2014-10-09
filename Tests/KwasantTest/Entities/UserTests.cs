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
    public class UserTests : BaseTest
    {
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
                var fixture = new FixtureData(uow);
                var role = new Role();
                role.Add(uow, fixture.TestRole());
                var u = new UserDO();
                var user = new User();

                //SETUP                 

                UserDO currUserDO = new UserDO();
                uow.UserRepository.Add(currUserDO);
               
                UserDO currRetrivedUserDO = uow.UserRepository.GetQuery().FirstOrDefault(uu => currUserDO.EmailAddressID == uu.EmailAddressID);
            }
        }
    }
}
