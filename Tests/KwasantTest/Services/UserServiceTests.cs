using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
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
        public void CanGetAllUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Adding test Users
                UserDO testUser1 = new UserDO() { Id = "1", FirstName = "Firsttest1", LastName = "Lasttest1", EmailAddress = new EmailAddressDO { Address = "alex@kwasant.com" } };
                uow.UserRepository.Add(testUser1);
                UserDO testUser2 = new UserDO() { Id = "2", FirstName = "Firsttest2", LastName = "Lasttest2", EmailAddress = new EmailAddressDO { Address = "alex@gmail.com" } };
                uow.UserRepository.Add(testUser2);
                UserDO testUser3 = new UserDO() { Id = "3", FirstName = "Firsttest3", LastName = "Lasttest3", EmailAddress = new EmailAddressDO { Address = "alexkwa@sant.com" } };
                uow.UserRepository.Add(testUser3);
                uow.SaveChanges();

                //Getting all users
                Assert.AreEqual(3, new User().Query(uow, "").Count());
            }
        }
    }
}
