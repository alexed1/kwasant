﻿using System;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Controllers
{
    public class UserControllerTests : BaseTest
    {

        [Test]
        public void ShowAllTestNoUsers()
        {
            //Database starts empty - we have no users

            var controller = new UserController();
            //Check we get a view back
            var res = controller.Index() as ViewResult;
            Assert.NotNull(res);

            //Check the view has a model
            var model = res.Model as UserShowAllVM;
            Assert.NotNull(model);

            //Check we have no users
            Assert.AreEqual(0, model.Users.Count);
        }

        [Test]
        public void ShowAllTestWithUserNoRoles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a user, do not give him roles
                uow.UserRepository.GetOrCreateUser("rjrudman@gmail.com");
                uow.SaveChanges();
            }

            var controller = new UserController();
            //Check we get a view back
            var res = controller.Index() as ViewResult;
            Assert.NotNull(res);

            //Check the view has a model
            var model = res.Model as UserShowAllVM;
            Assert.NotNull(model);

            //Check we have a user
            Assert.AreEqual(1, model.Users.Count);

            //Check he has no roles
            Assert.AreEqual(null, model.Users.First().Role);
        }


        [Test]
        public void ShowAllTestWithUserWithRoles()
        {
            string roleName = "Admin";
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a user
                var userDO = uow.UserRepository.GetOrCreateUser("rjrudman@gmail.com");

                //Create a role
                uow.AspNetRolesRepository.Add(new AspNetRolesDO {Name = roleName});

                //Assign user to role
                uow.AspNetUserRolesRepository.AssignRoleToUser(roleName, userDO.Id);
                uow.SaveChanges();
            }

            var controller = new UserController();
            //Check we get a view back
            var res = controller.Index() as ViewResult;
            Assert.NotNull(res);
            
            //Check the view has a model
            var model = res.Model as UserShowAllVM;
            Assert.NotNull(model);

            //Check we have a user
            Assert.AreEqual(1, model.Users.Count);

            //Check he has the correct role
            Assert.AreEqual(roleName, model.Users.First().Role);
        } 
    }
}