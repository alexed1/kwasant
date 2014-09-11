using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace KwasantCore.Services
{
    public class Account
    {
        private static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public RegistrationStatus ProcessRegistrationRequest(String email, String password)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RegistrationStatus curRegStatus = RegistrationStatus.Pending;
                var isNewUser = false;
                UserDO newUserDO = null;
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
                        newUserDO = Register(uow, email, email, email, password, "Customer");
                        curRegStatus = RegistrationStatus.Successful;
                    }
                }
                else
                {
                    newUserDO = Register(uow, email, email, email, password, "Customer");
                    curRegStatus = RegistrationStatus.Successful;
                }

                uow.SaveChanges();

                if (newUserDO != null)
                {
                    //AlertManager.CustomerCreated(newUserDO);
                    AlertManager.UserRegistration(newUserDO);
                }
               
                return curRegStatus;
            }
        }

        public async Task<LoginStatus> ProcessLoginRequest(string username, string password, bool isPersistent)
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
                        //if (userDO.EmailConfirmed)
                        //{
                        //    curLoginStatus = await Login(uow, username, password, isPersistent);
                        //}
                        curLoginStatus = await Login(uow, username, password, isPersistent);
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


        public UserDO Register(IUnitOfWork uow, string userName, string firstName, string lastName, string password, string role)
        {
            UserDO userDO = new UserDO();
            try
            {
                EmailAddressDO curEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(userName);

                userDO = uow.UserRepository.CreateFromEmail(
                    emailAddressDO: curEmailAddress,
                    userName: userName,
                    firstName: firstName,
                    lastName: lastName);

                UserManager<UserDO> userManager = KwasantCore.Services.User.GetUserManager(uow); ;
                IdentityResult result = userManager.Create(userDO, password);
                if (result.Succeeded)
                {
                    userManager.AddToRole(userDO.Id, role);
                }
                else
                {
                    throw new ApplicationException("There was a problem trying to register you. Please try again.");
                }
            }
            catch (Exception ex)
            {
                LogRegistrationError(ex);
            }
            return userDO;
        }

        public async Task<LoginStatus> Login(IUnitOfWork uow, string username, string password, bool isPersistent)
        {
            LoginStatus curLogingStatus = LoginStatus.Successful;
            UserManager<UserDO> curUserManager = KwasantCore.Services.User.GetUserManager(uow); ;
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

        public void LogRegistrationError(Exception ex)
        {
            IncidentDO incidentDO = new IncidentDO();
            incidentDO.PrimaryCategory = "Error";
            incidentDO.SecondaryCategory = "Processing";
            incidentDO.CreateTime = DateTime.Now;
            incidentDO.Activity = "Registration";
            incidentDO.Notes = ex.Message;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }

            string logData = string.Format("{0} {1} {2}:" + " ObjectId: {3} CustomerId: {4}",
                    incidentDO.PrimaryCategory,
                    incidentDO.SecondaryCategory,
                    incidentDO.Activity,
                    incidentDO.ObjectId,
                    incidentDO.CustomerId);

            Logger.GetLogger().Info(logData);
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