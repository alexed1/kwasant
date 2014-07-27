using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;
using Utilities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace KwasantCore.Services
{
    public class Account
    {
        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public RegistrationStatus Register(String email, String password)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RegistrationStatus curRegStatus = RegistrationStatus.Pending;
                //check if we know this email address

                EmailAddressDO existingEmailAddressDO = uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == email);
                if (existingEmailAddressDO != null)
                {
                    var existingUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddressID == existingEmailAddressDO.Id);
                    if (existingUserDO != null)
                    {
                        if (existingUserDO.PasswordHash == null)
                        {
                            //this is an existing implicit user, who sent in a request in the past, had a UserDO created, and now is registering. Add the password
                            new User().UpdatePassword(uow, existingUserDO, password);
                            curRegStatus = RegistrationStatus.Successful;
                        }
                        else
                        {
                            //tell 'em to login
                            curRegStatus = RegistrationStatus.UserMustLogIn;
                        }
                    }
                    else
                    {
                        var userDO = Register(uow, email, password, "Customer");
                        curRegStatus = RegistrationStatus.Successful;
                    }
                }
                else
                {
                    var userDO = Register(uow, email, password, "Customer");
                    curRegStatus = RegistrationStatus.Successful;
                }

                uow.SaveChanges();

                return curRegStatus;
            }
        }

        private UserDO Register(IUnitOfWork uow, string email, string password, string role)
        {
            var user = new User();
            return user.Register(uow, email, password, role);
        }

        public async Task<LoginStatus> Login(string username, string password, bool isPersistent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                LoginStatus curLoginStatus = LoginStatus.Pending;

                UserDO userDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (userDO != null)
                {
                    if (string.IsNullOrEmpty(userDO.PasswordHash))
                    {
                        curLoginStatus = LoginStatus.ImplicitUser;
                    }
                    else
                    {
                        if (userDO.EmailConfirmed)
                        {
                            curLoginStatus = await new User().Login(uow, username, password, isPersistent);
                        }
                    }
                }
                else
                {
                    curLoginStatus = LoginStatus.UnregisteredUser;
                }

                return curLoginStatus;
            }
        }

        public void LogOff()
        {
            new User().LogOff();
        }

        //this doesn't seem to get called. let's watch for a while and then delete it
        //public void UpdateUser(UserDO userDO, IdentityUserRole identityUserRole)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        EmailAddressDO currEmailAddressDO = uow.EmailAddressRepository.GetByKey(userDO.EmailAddressID);
        //        currEmailAddressDO.Address = userDO.EmailAddress.Address;

        //        //Change user's role in DB using Identity Framework if only role is changed on the fone-end.
        //        if (identityUserRole != null)
        //        {
        //            User user = new User();
        //            user.ChangeUserRole(uow, identityUserRole);
        //        }
        //        uow.SaveChanges();
        //    }
        //}
    }
}