using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Configuration;

using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Infrastructure;
using UtilitiesLib;
using KwasantCore.Managers.IdentityManager;

namespace KwasantCore.Services
{
    public class Account
    {
        private UserRepository _userRepo;
        private PersonRepository _personRepo;
        private IdentityManager _identityManager;

        public Account(IUnitOfWork uow)
        {
            _userRepo = new UserRepository(uow);
            _personRepo = new PersonRepository(uow);
            _identityManager = new IdentityManager(uow);
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<RegistrationStatus> Register(UserDO userDO)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Successful;
            UserDO user = GetUser(userDO.UserName); //check this user already exists in DB or not

            if (user != null) // Existing user
            {
                curRegStatus = _identityManager.RegisterExistingUser(userDO);
            }
            else
            {
                //check this user already exists in DB or not
                PersonDO curPerson = GetPerson(userDO.UserName);
                if (curPerson != null) // Person exists. Convert Person to UserDO.
                {
                    userDO = await _identityManager.ConvertExistingPerson(curPerson, userDO.UserName, userDO.Password);
                }
                else // add new user
                {
                    curRegStatus = await _identityManager.RegisterNewUser(userDO);
                }
            }

            return curRegStatus;
        }

        public async Task<LoginStatus> Login(UserDO userDO, bool isPersistent)
        {
            LoginStatus curLoginStatus = LoginStatus.Successful;

            UserDO user = GetUser(userDO.UserName);
            if (user != null)
            {
                if (user.Password.Length == 0)
                {
                    curLoginStatus = LoginStatus.ImplicitUser;
                }
                else
                {
                    if (user.EmailConfirmed)
                    {
                        userDO.EmailConfirmed = true;
                        curLoginStatus = await _identityManager.Login(userDO, isPersistent);
                    }
                }
            }
            else
            {
                curLoginStatus = LoginStatus.UnregisteredUser;
            }

            return curLoginStatus;
        }

        public void LogOff()
        {
            _identityManager.LogOff();
        }

        /// <summary>
        /// Check user exists or not
        /// </summary>
        /// <param name="userDO"></param>
        /// <returns></returns>
        private UserDO GetUser(string userName)
        {
            return _userRepo.FindOne(x => x.UserName == userName);
        }

        /// <summary>
        /// Check person exists or not
        /// </summary>
        /// <returns></returns>
        private PersonDO GetPerson(string userName)
        {
            return _personRepo.FindOne(x => x.EmailAddress.Address == userName);
        }
    }
}