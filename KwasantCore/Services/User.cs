using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Validators;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using Data.Entities;
using StructureMap;
using Utilities;

namespace KwasantCore.Services
{
    public class User
    {
        private static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public UserDO Register (IUnitOfWork uow, string userName, string password, string role)
        {
            var userDO = new UserDO();
            userDO.FirstName = userName;
            userDO.LastName = userName;
            userDO.UserName = userName;
            uow.UserRepository.Add(userDO);

            UserValidator curUserValidator = new UserValidator();
            curUserValidator.ValidateAndThrow(userDO);

            UserManager<UserDO> userManager = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));
            IdentityResult result = userManager.Create(userDO, password);
            if (result.Succeeded)
            {
                userManager.AddToRole(userDO.Id, role);
            }
            else
            {
                throw new ApplicationException("There was a problem trying to register you. Please try again.");
            }

            return userDO;
        }

        public void UpdatePassword(IUnitOfWork uow, UserDO userDO, string password)
        {
            if (userDO != null)
            {
                UserManager<UserDO> curUserManager = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));

                curUserManager.RemovePassword(userDO.Id); //remove old password
                var curResult = curUserManager.AddPassword(userDO.Id, password); // add new password
                if (curResult.Succeeded == false)
                {
                    throw new ApplicationException("There was a problem trying to change your password. Please try again.");
                }
            }
        }

        public async Task<LoginStatus> Login(string username, string password, bool isPersistent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                LoginStatus curLogingStatus = LoginStatus.Successful;
                UserManager<UserDO> curUserManager = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));
                UserDO curUser = await curUserManager.FindAsync(username, password);
                if (curUser != null)
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                    ClaimsIdentity identity = await curUserManager.CreateIdentityAsync(curUser, DefaultAuthenticationTypes.ApplicationCookie);

                    if (identity.IsAuthenticated == false)
                    {
                        throw new ApplicationException("There was an error logging in. Please try again later.");
                    }
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = isPersistent
                    }, identity);
                }
                else
                {
                    curLogingStatus = LoginStatus.InvalidCredential;
                }

                return curLogingStatus;
            }
        }

        public void LogOff()
        {
            AuthenticationManager.SignOut();
        }
        
        public bool ChangeUserRole(IUnitOfWork uow, IdentityUserRole identityUserRole)
        {
            UserManager<UserDO> userManager = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(uow.Db));

            IList<string> currCurrentIdentityRole = userManager.GetRoles(identityUserRole.UserId);
            IdentityResult identityResult = userManager.RemoveFromRole(identityUserRole.UserId, currCurrentIdentityRole.ToList()[0]);

            if (identityResult.Succeeded)
            {
                IdentityRole currNewIdentityRole = roleManager.FindById(identityUserRole.RoleId.Trim());
                identityResult = userManager.AddToRole(identityUserRole.UserId.Trim(), currNewIdentityRole.Name);
            }

            return identityResult.Succeeded;
        }
    }
}
