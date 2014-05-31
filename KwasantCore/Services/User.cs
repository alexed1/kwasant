using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
//using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Web;
using Microsoft.Owin.Host.SystemWeb;
using System.Data.Entity;

using Data.Interfaces;
using Data.Entities;
using StructureMap;
using Data.Repositories;
using Utilities;
using Data.Infrastructure;

namespace KwasantCore.Services
{
    public class User
    {
        private readonly IUnitOfWork _uow;
        private UserRepository userRepo;        

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public User(IUnitOfWork uow)
        {
            _uow = uow;            
        }

        public void Add(UserDO userDO)
        {
            userRepo.Add(userDO);
        }

        public async Task<RegistrationStatus> Create(UserDO userDO, string role)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Successful;

            var userManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));
            var result = await userManager.CreateAsync(userDO, userDO.Password);
            if (result.Succeeded)
            {
                userManager.AddToRole(userDO.Id, role);
            }
            else
            {
                throw new ApplicationException("There was a problem trying to register you. Please try again.");
            }

            return curRegStatus;
        }

        public void UpdatePassword(UserDO userDO)
        {
            if (userDO != null)
            {
                var curUserManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));

                IdentityResult curResult = curUserManager.RemovePassword(userDO.Id); //remove old password
                curResult = curUserManager.AddPassword(userDO.Id, userDO.Password); // add new password
                if (curResult.Succeeded == false)
                {
                    throw new ApplicationException("There was a problem trying to change your password. Please try again.");
                }
            }
        }

        public async Task<LoginStatus> Login(UserDO userDO, bool isPersistent)
        {
            LoginStatus curLogingStatus = LoginStatus.Successful;
            var curUserManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));
            var curUser = await curUserManager.FindAsync(userDO.UserName, userDO.Password);
            if (curUser != null)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                var identity = await curUserManager.CreateIdentityAsync(
                                curUser, DefaultAuthenticationTypes.ApplicationCookie);

                if (identity.IsAuthenticated == false)
                {
                    throw new ApplicationException("There was an error logging in. Please try again later.");
                }
                AuthenticationManager.SignIn(
                   new AuthenticationProperties()
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

        public void LogOff()
        {
            AuthenticationManager.SignOut();
        }

        public UserDO FindByEmailId(int Id)
        {
            return userRepo.FindOne(p => p.EmailAddress.Id == Id);

        }

        public bool ChangeUserRole(IdentityUserRole identityUserRole)
        {
            IdentityResult identityResult = null;            

            var userManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_uow.Db as KwasantDbContext));

            var currCurrentIdentityRole = userManager.GetRoles(identityUserRole.UserId);
            identityResult = userManager.RemoveFromRole(identityUserRole.UserId, currCurrentIdentityRole.ToList()[0]);

            if (identityResult.Succeeded)
            {
                IdentityRole currNewIdentityRole = roleManager.FindById(identityUserRole.RoleId.Trim());
                identityResult = userManager.AddToRole(identityUserRole.UserId.Trim(), currNewIdentityRole.Name);
            }

            return identityResult.Succeeded;
        }
    }
}
