using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Infrastructure;
using FluentValidation;
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

    }
}
