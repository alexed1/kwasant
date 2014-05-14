using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Web.Mvc;
using System.Threading.Tasks;

using StructureMap;
using Data.Repositories;
using Data.Interfaces;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using UtilitiesLib;

namespace KwasantTest.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        public IUserRepository _userRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _userRepo = new UserRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        public void AccountControllerCanRegister()
        {
            AccountController curController = new AccountController(_uow);
            RegisterViewModel curViewModel = new RegisterViewModel();
            curViewModel.Email = "pabitra@hotmail.com";
            curViewModel.Password = "abcd1234";

            var curTaskResult = curController.Register(curViewModel);
            Assert.IsNotNull(curTaskResult);

            ActionResult curActionResult = curTaskResult.Result;
            Assert.IsNotNull(curActionResult);
            //Assert.AreEqual("Confirm", curActionResult.["action"].);
        }
    }
}
 