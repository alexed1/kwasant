﻿using System;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Authentication
{
    class Account
    {

        //creates a user but does not assign it a role
        public string Create(string curUserName, string curPassword,IUnitOfWork unitOfWork)
        {

            var um = new UserManager<UserDO>(new UserStore<UserDO>(unitOfWork.Db as KwasantDbContext));
            UserDO curUser = um.FindByName(curUserName);
            if (curUser == null)
            {
                curUser = new UserDO()
                {
                    UserName = curUserName,
                    EmailAddress = unitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(curUserName),
                    FirstName = curUserName,
                    EmailConfirmed = true,
                    TestAccount = true
                };

                AddDefaultCalendar(curUser);

                IdentityResult ir = um.Create(curUser, curPassword);

                if (!ir.Succeeded)
                    return "failure"; //this is smelly

               
            }
            else
            {
                curUser.TestAccount = true;
                //This line forces EF to load the EmailAddress, since it's done lazily. For whatever reason, seeding admins breaks since it thinks the EmailAddress is null...
                var forceEmail = curUser.EmailAddress;
                //This line forces the above not to be optimised out. In production, it does nothing.
                
            }
        
            return curUser.Id;
        }

        public void AddDefaultCalendar(UserDO curUser)
        {
            if (curUser == null)
                throw new ArgumentNullException("curUser");

            var curCalendar = new CalendarDO
            {
                Name = "Default Calendar",
                Owner = curUser,
                OwnerID = curUser.Id
            };
            curUser.Calendars.Add(curCalendar);
        }

        public void AddRole(string roleName, string accountId, IUnitOfWork unitOfWork)
        {
            var um = new UserManager<UserDO>(new UserStore<UserDO>(unitOfWork.Db as KwasantDbContext));
            um.AddToRole(accountId, roleName);

        }

        public void RemoveRole(string roleName, string accountId, IUnitOfWork unitOfWork)
        {
            var um = new UserManager<UserDO>(new UserStore<UserDO>(unitOfWork.Db as KwasantDbContext));
            um.RemoveFromRole(accountId, roleName);

        }


    }
}
