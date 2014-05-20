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
using StructureMap;
using UtilitiesLib;
using KwasantCore.Managers.IdentityManager;

namespace KwasantCore.Services
{
    public class Account
    {
        private UserRepository _userRepo;
        private PersonRepository _personRepo;
        private IdentityManager _identityManager;
        private IUnitOfWork _uow;
        private User _curUser;
        private Person _curPerson;

        public Account(IUnitOfWork uow) //remove injected uow. unnecessary now.
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _userRepo = new UserRepository(_uow);
            _personRepo = new PersonRepository(_uow);
            _identityManager = new IdentityManager(_uow);
            _curUser = new User(_uow);
            _curPerson = new Person(_uow);
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<RegistrationStatus> Register(UserDO userRegStrings)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Pending; 

<<<<<<< HEAD
            if (user != null) // Existing user
            {
                curRegStatus = _identityManager.RegisterExistingUser(userDO);
            }
            else
            {
                //check this user already exists in DB or not
                PersonDO curPerson = GetPerson(userDO.UserName);
                if (curPerson != null) // Person exists. Convert Person to KwasantUserDO.
                {
                    userDO = await _identityManager.ConvertExistingPerson(curPerson, userDO.UserName, userDO.Password);
                }
                else // add new user
                {
                    curRegStatus = await _identityManager.RegisterNewUser(userDO);
                }
=======


            //check if we know this email address
            EmailAddress curEmailAddress = new EmailAddress();
            EmailAddressDO existingEmailAddressDO = curEmailAddress.FindByAddress(userRegStrings.Email);
            if (existingEmailAddressDO != null)
            {
                //this should be improved. doesn't take advantage of inheritance.
                
                PersonDO curPersonDO = _curPerson.FindByEmailId(existingEmailAddressDO.Id);            
                UserDO curUserDO = _curUser.FindByEmailId(existingEmailAddressDO.Id);

                if (curUserDO != null)
                {
                    //if a User, redirect to an error message
                }
                else  //existingEmailAddressDO is Person
                {
                    //create a new User and delete the corresponding Person
                    curUserDO = await _identityManager.ConvertExistingPerson(curPersonDO, userRegStrings);
                    curRegStatus = RegistrationStatus.Successful;
                }
            }
            else 
            {
                //this email address unknown.  new user. create an EmailAddress object, then create a User
                curRegStatus = await _identityManager.RegisterNewUser(userRegStrings);
                curRegStatus = RegistrationStatus.Successful;
>>>>>>> origin/kw-101
            }

            return curRegStatus;
        }

        public async Task<LoginStatus> Login(UserDO userDO, bool isPersistent)
        {
            LoginStatus curLoginStatus = LoginStatus.Pending;

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
        /// <param name="KwasantUserDO"></param>
        /// <returns></returns>
        private UserDO GetUser(string userName)
        {
            return _userRepo.FindOne(x => x.UserName == userName);
        }


    }
}