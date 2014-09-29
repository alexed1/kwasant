using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    class UserServiceTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test]
        [Category("User")]
        public void CanSearchUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Adding test Users
                UserDO testUser1 = new UserDO() { FirstName = "Firsttest1", LastName = "Lasttest1", EmailAddress = new EmailAddressDO { Address = "alex@kwasant.com" } };
                uow.UserRepository.Add(testUser1);
                UserDO testUser2 = new UserDO() { FirstName = "Firsttest2", LastName = "Lasttest2", EmailAddress = new EmailAddressDO { Address = "alex@gmail.com" } };
                uow.UserRepository.Add(testUser2);

                //Searching for users
                UserDO searchQueryUser = new UserDO() { FirstName = "test" };
                Assert.AreEqual(2, new User().Query(uow, searchQueryUser).Count());

                searchQueryUser = new UserDO() { EmailAddress = new EmailAddressDO { Address = "alex@kwasant.com" } };
                Assert.AreEqual(1, new User().Query(uow, searchQueryUser).Count());

                searchQueryUser = new UserDO() { FirstName = "fail" };
                Assert.AreEqual(0, new User().Query(uow, searchQueryUser).Count());
            }
        }
    }
}
