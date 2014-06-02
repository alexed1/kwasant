using System;
using System.Collections.Generic;
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
        private IUnitOfWork _uow;
        private FixtureData _fixture;
        private Role _role;
        private User _user;
        private IUserRepository userRepo;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _role = new Role(_uow);
            userRepo = _uow.UserRepository;
            _user = new User(_uow);
            _fixture = new FixtureData();
        }

        public UserDO SetupUserForTests()
        {
            AspNetRolesDO currAspNetRolesDO = _fixture.TestRole();
            _role.Add(currAspNetRolesDO);

            return new UserDO();
        }

        [Test]
        [Category("User")]
        public void CanAddUser()
        {
            UserDO u = SetupUserForTests();

            //SETUP                 

            UserDO currUserDO = new UserDO();
            _user.Add(currUserDO);

            UserDO currRetrivedUserDO = _user.FindByEmailId(currUserDO.EmailAddressID);
        }

    }
}
