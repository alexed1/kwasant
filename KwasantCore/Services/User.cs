﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using Data.Entities;
using StructureMap;
using Utilities;
using Data.States;

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

        public UserDO GetOrCreateFromBR(IUnitOfWork uow, EmailAddressDO emailAddressDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (emailAddressDO == null)
                throw new ArgumentNullException("emailAddressDO");
            UserDO curUser = Get(uow, emailAddressDO);
            if (curUser == null)
            {
                curUser = uow.UserRepository.CreateFromEmail(emailAddressDO);
               
            }
            return curUser;
        }

        public UserDO Get(IUnitOfWork uow, BookingRequestDO bookingRequestDO)
        {
            if (bookingRequestDO == null)
                throw new ArgumentNullException("bookingRequestDO");
            return Get(uow, bookingRequestDO.From);
        }

        public UserDO Get(IUnitOfWork uow, EmailAddressDO emailAddressDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (emailAddressDO == null)
                throw new ArgumentNullException("emailAddressDO");
            return uow.UserRepository.GetByEmailAddress(emailAddressDO);
        }


        public UserDO Register (IUnitOfWork uow, string userName, string password, string role)
        {

            EmailAddressDO curEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(userName);
             
            var userDO =uow.UserRepository.CreateFromEmail(
                emailAddressDO:  curEmailAddress,
                userName: userName,
                firstName: userName,
                lastName: userName);

            UserManager<UserDO> userManager = GetUserManager(uow);;
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

        public UserDO Register(IUnitOfWork uow, string userName, string firstName, string lastName, string password, string role)
        {

            EmailAddressDO curEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(userName);

            var userDO = uow.UserRepository.CreateFromEmail(
                emailAddressDO: curEmailAddress,
                userName: userName,
                firstName: firstName,
                lastName: lastName);

            UserManager<UserDO> userManager = GetUserManager(uow); ;
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
                UserManager<UserDO> curUserManager = GetUserManager(uow); ;

                curUserManager.RemovePassword(userDO.Id); //remove old password
                var curResult = curUserManager.AddPassword(userDO.Id, password); // add new password
                if (curResult.Succeeded == false)
                {
                    throw new ApplicationException("There was a problem trying to change your password. Please try again.");
                }
            }
        }

        public async Task<LoginStatus> Login(IUnitOfWork uow, string username, string password, bool isPersistent)
        {
            LoginStatus curLogingStatus = LoginStatus.Successful;
            UserManager<UserDO> curUserManager = GetUserManager(uow);;
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

        public void LogOff()
        {
            AuthenticationManager.SignOut();
        }
        
        //problem: this assumes a single role but we need support for multiple roles on one account
        //problem: the line between account and user is really murky. do we need both?
        public bool ChangeUserRole(IUnitOfWork uow, IdentityUserRole identityUserRole)
        {
            UserManager<UserDO> userManager = GetUserManager(uow);
            RoleManager<IdentityRole> roleManager = Role.GetRoleManager(uow);

            IList<string> currCurrentIdentityRole = userManager.GetRoles(identityUserRole.UserId);
            IdentityResult identityResult = userManager.RemoveFromRole(identityUserRole.UserId, currCurrentIdentityRole.ToList()[0]);

            if (identityResult.Succeeded)
            {
                IdentityRole currNewIdentityRole = roleManager.FindById(identityUserRole.RoleId.Trim());
                identityResult = userManager.AddToRole(identityUserRole.UserId.Trim(), currNewIdentityRole.Name);
            }

            return identityResult.Succeeded;
        }

        public static UserManager<UserDO> GetUserManager(IUnitOfWork uow)
        {
            var userStore = ObjectFactory.GetInstance<IKwasantUserStore>();
            var um = new UserManager<UserDO>(userStore.SetUnitOfWork(uow));
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Sample");
            um.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<UserDO>(provider.Create("EmailConfirmation"));

            return um;
        }


        //
        //get roles for this User
        //if at least one role meets or exceeds the provided level, return true, else false
        public bool VerifyMinimumRole(string minAuthLevel, string curUserId, IUnitOfWork uow)
        {
            var um = GetUserManager(uow);
            var roles = um.GetRoles(curUserId);
            String[] acceptableRoles = {};
            switch (minAuthLevel)
            {
                case "Customer":
                    acceptableRoles = new[] {"Customer", "Booker", "Admin"};
                    break;
                case "Booker":
                    acceptableRoles = new[] {"Booker", "Admin"};
                    break;
                case "Admin":
                    acceptableRoles = new[] {"Admin"};
                    break;
            }
            //if any of the roles that this user belongs to are contained in the current set of acceptable roles, return true
            if (roles.Any(role => acceptableRoles.Contains(role)))
                        return true;
                    return false;
         

        }




        public UserDO GetUser(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetAll().Where(e => e.Id == curUserId).FirstOrDefault();
                return new UserDO
                {
                    Id = curUser.Id,
                    Calendars = curUser.Calendars,
                    Email = curUser.Email,
                    EmailAddress = curUser.EmailAddress,
                    FirstName = curUser.FirstName,
                    LastName = curUser.LastName
                };
    }
}

        public string GetRole(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curManager = User.GetUserManager(uow);
                return curManager.GetRoles(curUserId)[0];
            }
        }

        public List<UserDO> Query(IUnitOfWork uow, UserDO curUserSearch)
        {
            return uow.UserRepository.GetAll().ToList().Where(e =>
                  curUserSearch.FirstName != null ?
                  e.FirstName != null ?
                  e.FirstName.Contains(curUserSearch.FirstName) : false : false ||
                  curUserSearch.LastName != null ?
                  e.LastName != null ?
                  e.LastName.Contains(curUserSearch.LastName) : false : false ||
                  curUserSearch.EmailAddress.Address != null ?
                  e.EmailAddress.Address != null ?
                  e.EmailAddress.Address.Contains(curUserSearch.EmailAddress.Address) : false : false
                  ).ToList();
        }

        public void Create(IUnitOfWork uow, UserDO curUser, string role, bool sendEmail)
        {
            if (sendEmail)
            {
                EmailDO curEmail = new EmailDO();
                curEmail.From = curUser.EmailAddress;
                curEmail.AddEmailRecipient(EmailParticipantType.To, curUser.EmailAddress);
                curEmail.Subject = "User Settings Notification";
                new Email(uow).SendTemplate("User_Settings_Notification", curEmail, null);
            }
            Register(uow, curUser.EmailAddress.Address, curUser.FirstName, curUser.LastName, "test@1234", role);
        }

        public void Update(IUnitOfWork uow, UserDO curUser, string role)
        {
            EmailAddressDO mailaddress = uow.EmailAddressRepository.GetAll().Where(e => e.Address == curUser.EmailAddress.Address).FirstOrDefault();
            if (mailaddress != null)
            {
                UserDO existingUser = uow.UserRepository.GetAll().Where(e => e.EmailAddress.Address == curUser.EmailAddress.Address).FirstOrDefault();
                existingUser.FirstName = curUser.FirstName;
                existingUser.LastName = curUser.LastName;
                existingUser.EmailAddress.Address = curUser.EmailAddress.Address;
                var curManager = User.GetUserManager(uow);
                curManager.AddToRole(existingUser.Id, role);
                uow.SaveChanges();
            }
        }
    }
}

