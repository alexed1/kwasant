using System;
using System.Linq;
using System.Threading.Tasks;
using System.EnterpriseServices;
using Data.Entities;
using Data.Interfaces;
using Data.Infrastructure;
using Data.Repositories;
using StructureMap;
using Utilities;
using KwasantCore.Managers.IdentityManager;
using AutoMapper;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace KwasantCore.Services
{
    public class Account
    {
        private IdentityManager _identityManager;
        private IUnitOfWork _uow;
        private User _curUser;
        private Role _role;
        private IUserRepository userRepo;
        private IEmailAddressRepository emailAddressRepo;

        public Account(IUnitOfWork uow) //remove injected uow. unnecessary now.
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _identityManager = new IdentityManager(_uow);
            _curUser = new User(_uow);
            _role = new Role(_uow);
            userRepo = _uow.UserRepository;
            emailAddressRepo = _uow.EmailAddressRepository;
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<RegistrationStatus> Register(UserDO userRegStrings)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Pending;

            //check if we know this email address

            EmailAddressDO existingEmailAddressDO = _uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == userRegStrings.Email);
            if (existingEmailAddressDO != null)
            {
                //this should be improved. doesn't take advantage of inheritance.
                UserDO curUserDO = _curUser.FindByEmailId(existingEmailAddressDO.Id);
                if (curUserDO != null)
                {

                    if (curUserDO.Password == null)
                    {
                        //this is an existing implicit user, who sent in a request in the past, had a UserDO created, and now is registering. Add the password
                        curUserDO.Password = userRegStrings.Password;
                        _identityManager.AttachPassword(curUserDO);
                    }
                    else
                    {
                        //tell 'em to login
                        curRegStatus = RegistrationStatus.UserMustLogIn;
                    }
                }
                else  //existingEmailAddressDO is Person
                {
                    curRegStatus = RegistrationStatus.Successful;
                }
            }
            else
            {
                //this email address unknown.  new user. create an EmailAddress object, then create a User
                curRegStatus = await _identityManager.RegisterNewUser(userRegStrings);
                curRegStatus = RegistrationStatus.Successful;
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
            return _uow.UserRepository.FindOne(x => x.UserName == userName);
        }


        public void UpdateUser(UserDO userDO, IdentityUserRole identityUserRole)
        {
            userRepo.UnitOfWork.Db.Entry(userDO).State = System.Data.Entity.EntityState.Modified;

            EmailAddressDO currEmailAddressDO = new EmailAddressDO();
            currEmailAddressDO = emailAddressRepo.GetByKey(userDO.EmailAddressID);
            currEmailAddressDO.Address = userDO.EmailAddress.Address;
            emailAddressRepo.UnitOfWork.Db.Entry(currEmailAddressDO).State = System.Data.Entity.EntityState.Modified;
            _uow.SaveChanges();
            
            //Change user's role in DB using Identity Framework if only role is changed on the fone-end.
            if (identityUserRole != null)
            {
                User user = new User(_uow);
                user.ChangeUserRole(identityUserRole);
            }            
        }
    }
}