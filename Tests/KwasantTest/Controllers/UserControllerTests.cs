using System;
using System.Collections.Generic;
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
            var usersModel = GetAllUsers();

            //Check we have no users
            Assert.AreEqual(0, usersModel.Count);
        }

        [Test]
        public void ShowAllTestWithUserNoRoles()
        {
            CreateUser("rjrudman@gmail.com");

            var usersModel = GetAllUsers();
            
            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have no roles
            var userVM = usersModel.First();
            Assert.AreEqual(0, userVM.Roles.Count);
        }

        [Test]
        public void ShowAllTestWithUserWithOneRole()
        {
            const string roleName = "Admin";
            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, roleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            Assert.AreEqual(1, usersModel.First().Roles.Count);
            var firstRole = usersModel.First().Roles.First();
            Assert.AreEqual(roleName, firstRole);
        }

        [Test]
        public void ShowAllTestWithUserWithMultipleRoles()
        {
            const string firstRoleName = "Admin";
            const string secondRoleName = "Booker";
            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, firstRoleName);
            AssignRoleToUser(userDO, secondRoleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly two roles
            Assert.AreEqual(2, usersModel.First().Roles.Count);
            var firstRole = usersModel.First().Roles.First();
            Assert.AreEqual(firstRoleName, firstRole);

            var secondRole = usersModel.First().Roles[1];
            Assert.AreEqual(secondRoleName, secondRole);
        }

        [Test]
        public void TestUpdateUserBasicDetails()
        {
            CreateUser("rjrudman@gmail.com");

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);
            var firstUserModel = usersModel.First();

            Assert.IsNull(firstUserModel.FirstName);

            firstUserModel.FirstName = "Rob";
            UpdateUser(firstUserModel);

            //Now check users after updating
            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);
            firstUserModel = usersModel.First();

            Assert.AreEqual("Rob", firstUserModel.FirstName);
        }

        [Test]
        public void TestUpdateUserRoleAdd()
        {
            const string roleName = "Admin";
            CreateUser("rjrudman@gmail.com");
            CreateRole(roleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check he has no roles
            var userVM = usersModel.First();
            Assert.AreEqual(0, userVM.Roles.Count);

            userVM.Roles.Add(roleName);
            UpdateUser(userVM);

            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            Assert.AreEqual(1, usersModel.First().Roles.Count);
            var firstRole = usersModel.First().Roles.First();
            Assert.AreEqual(roleName, firstRole);
        }

        [Test]
        public void TestUpdateUserRoleRemove()
        {
            const string roleName = "Admin";
            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, roleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            var userVM = usersModel.First();

            Assert.AreEqual(1, userVM.Roles.Count);
            var firstRole = userVM.Roles.First();
            Assert.AreEqual(roleName, firstRole);

            userVM.Roles = new List<String>();
            UpdateUser(userVM);

            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have no roles
            userVM = usersModel.First();
            Assert.AreEqual(0, userVM.Roles.Count);
        }

        [Test]
        public void TestUpdateUserRoleAddAndRemove()
        {
            const string firstRoleName = "Admin";
            const string secondRoleName = "Booker";

            var userDO = CreateUser("rjrudman@gmail.com");
            AssignRoleToUser(userDO, firstRoleName);
            CreateRole(secondRoleName);

            var usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            var userVM = usersModel.First();
            
            Assert.AreEqual(1, userVM.Roles.Count);
            var firstRole = userVM.Roles.First();
            Assert.AreEqual(firstRoleName, firstRole);

            userVM.Roles.Clear();
            userVM.Roles.Add(secondRoleName);
            UpdateUser(userVM);

            usersModel = GetAllUsers();

            //Check we have a user
            Assert.AreEqual(1, usersModel.Count);

            //Check they have exactly one role
            Assert.AreEqual(1, userVM.Roles.Count);

            userVM = usersModel.First();
            firstRole = userVM.Roles.First();
            Assert.AreEqual(secondRoleName, firstRole);
        }

        [Test]
        public void TestDetail()
        {
            UserDO userDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a user
                userDO = uow.UserRepository.GetOrCreateUser("rjrudman@gmail.com");
                uow.SaveChanges();
            }

            var controller = new UserController();
            //Check we get a view back
            var res = controller.Details(userDO.Id) as ViewResult;
            Assert.NotNull(res);
        }

        private void UpdateUser(UserVM userVM)
        {
            var controller = new UserController();
            //Check we get a view back
            var res = controller.Update(userVM);
            Assert.NotNull(res);
        }

        private static List<UserVM> GetAllUsers()
        {
            var controller = new UserController();
            //Check we get a view back
            var res = controller.Index() as ViewResult;
            Assert.NotNull(res);

            //Check the view has a model
            var model = res.Model as List<UserVM>;
            Assert.NotNull(model);

            return model;
        }

        private static UserDO CreateUser(String emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a user, do not give him roles
                var userDO = uow.UserRepository.GetOrCreateUser(emailAddress);
                uow.SaveChanges();

                return userDO;
            }
        }

        private static void CreateRole(String roleID, String roleName = null)
        {
            if (roleName == null)
                roleName = roleID;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!uow.AspNetRolesRepository.GetQuery().Any(r => r.Name == roleID))
                {
                    //Create a role
                    uow.AspNetRolesRepository.Add(new AspNetRolesDO {Id = roleID, Name = roleName});
                    uow.SaveChanges();
                }
            }
        }

        private static void AssignRoleToUser(UserDO userDO, String roleID, String roleName = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CreateRole(roleID, roleName);
                //Assign user to role
                uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);
                uow.SaveChanges();
            }
        }
    }
}
