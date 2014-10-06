using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Security
{
    [TestFixture]
    public class RoleTests : BaseTest
    {
        [Test]
        public void DevUsersHaveAllRoles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var devEmails = new List<String>
                {
                    "alex@kwasant.com",
                    "pabitra@hotmail.com",
                    "rjrudman@gmail.com",
                    "quader.mamun@gmail.com",
                    "mkostyrkin@gmail.com"
                };

                var users = uow.UserRepository.GetQuery().Where(u => devEmails.Contains(u.EmailAddress.Address));
                foreach (var user in users)
                {
                    Assert.True(uow.AspNetUserRolesRepository.UserHasRole(Roles.Admin, user.Id), "User '" + user.EmailAddress.Address + "' does not have Admin role.");
                    Assert.True(uow.AspNetUserRolesRepository.UserHasRole(Roles.Booker, user.Id), "User '" + user.EmailAddress.Address + "' does not have Booker role.");
                    Assert.True(uow.AspNetUserRolesRepository.UserHasRole(Roles.Customer, user.Id), "User '" + user.EmailAddress.Address + "' does not have Customer role.");
                }
            }
        }
    }
}
