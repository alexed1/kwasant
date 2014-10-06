using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Security;
using Microsoft.AspNet.Identity;
using StructureMap;
using Utilities;
using Utilities.Logging;

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
        public RegistrationStatus ProcessRegistrationRequest(String email, String password)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RegistrationStatus curRegStatus;
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

        private UserDO ProcessRegistrationRequest(IUnitOfWork uow, string email, string password, string role)
        {
            var user = new User();
            return Register(uow, email, email, email, password, role);
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

        public UserDO Register(IUnitOfWork uow, string userName, string firstName, string lastName, string password, string role)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            return userDO;
        }

        public async Task<LoginStatus> Login(IUnitOfWork uow, string username, string password, bool isPersistent)
        {
            LoginStatus curLogingStatus = LoginStatus.Successful;
            UserManager<UserDO> curUserManager = User.GetUserManager(uow); ;
            UserDO curUser = await curUserManager.FindAsync(username, password);
            if (curUser != null)
            {
                var securityServices = ObjectFactory.GetInstance<ISecurityServices>();
                securityServices.Logout();
                securityServices.Login(uow, curUser);
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
    }
}