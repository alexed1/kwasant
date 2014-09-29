using System;
using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Validations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using StructureMap;
using Data.States;

namespace KwasantCore.Services
{
    public class User
    {
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

        /// <summary>
        /// Determines <see cref="CommunicationMode">communication mode</see> for user
        /// </summary>
        /// <param name="userDO">User</param>
        /// <returns>Direct if the user has a booking request or a password. Otherwise, Delegate.</returns>
        public CommunicationMode GetMode(UserDO userDO)
        {
            if (userDO.BookingRequests != null && userDO.BookingRequests.Any())
                return CommunicationMode.Direct;
            if(!String.IsNullOrEmpty(userDO.PasswordHash))
                return CommunicationMode.Direct;
            return CommunicationMode.Delegate;
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
            String[] acceptableRoles = { };
            switch (minAuthLevel)
            {
                case "Customer":
                    acceptableRoles = new[] { "Customer", "Booker", "Admin" };
                    break;
                case "Booker":
                    acceptableRoles = new[] { "Booker", "Admin" };
                    break;
                case "Admin":
                    acceptableRoles = new[] { "Admin" };
                    break;
            }
            //if any of the roles that this user belongs to are contained in the current set of acceptable roles, return true
            if (roles.Any(role => acceptableRoles.Contains(role)))
                        return true;
                    return false;
        }

        public UserDO Get(string curUserId)
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
                return curManager.GetRoles(curUserId).Count() > 0 ? curManager.GetRoles(curUserId)[0] : "";
            }
        }

        public List<UserDO> Query(IUnitOfWork uow, UserDO curUserSearch)
        {
            List<UserDO> filteredUsers = new List<UserDO>();
            uow.UserRepository.GetAll().Where(e =>
                  curUserSearch.FirstName != null ?
                  e.FirstName != null ?
                  e.FirstName.Contains(curUserSearch.FirstName) : false : false ||
                  curUserSearch.LastName != null ?
                  e.LastName != null ?
                  e.LastName.Contains(curUserSearch.LastName) : false : false ||
                  curUserSearch.EmailAddress.Address != null ?
                  e.EmailAddress.Address != null ?
                  e.EmailAddress.Address.Contains(curUserSearch.EmailAddress.Address) : false : false
                  ).ToList().ForEach(cur => filteredUsers.Add(new UserDO
                  {
                      FirstName = cur.FirstName,
                      LastName = cur.LastName,
                      EmailAddress = new EmailAddressDO
                      {
                          Address = cur.EmailAddress.Address,
                          Id = cur.EmailAddress.Id,
                          Name = cur.EmailAddress.Name
                      },
                      Id = cur.Id
                  }));
            return filteredUsers;
        }

        public void Create(IUnitOfWork uow, UserDO submittedUserData, string role, bool sendEmail)
        {
            if (sendEmail)
            {
                new Email().SendUserSettingsNotification(uow, submittedUserData);
            }
            new Account().Register(uow, submittedUserData.EmailAddress.Address, submittedUserData.FirstName, submittedUserData.LastName, "test@1234", role);
        }

        public void Update(IUnitOfWork uow, UserDO curUser, string role)
        {
            EmailAddressDO mailaddress = uow.EmailAddressRepository.GetAll().Where(e => e.Id == curUser.EmailAddress.Id).FirstOrDefault();
            if (mailaddress != null)
            {
                UserDO existingUser = uow.UserRepository.GetAll().Where(e => e.EmailAddress.Id == curUser.EmailAddress.Id).FirstOrDefault();
                existingUser.FirstName = curUser.FirstName;
                existingUser.LastName = curUser.LastName;
                existingUser.EmailAddress.Address = curUser.EmailAddress.Address;
                var curManager = User.GetUserManager(uow);
                IList<string> existingUserRoles = curManager.GetRoles(existingUser.Id);
                foreach (string exisitingRole in existingUserRoles)
                {
                    curManager.RemoveFromRole(existingUser.Id, exisitingRole);
                }
                curManager.AddToRole(existingUser.Id, role);
                uow.SaveChanges();
            }
        }

        //if we have a first name and last name, use them together
        //else if we have a first name only, use that
        //else if we have just an email address, use the portion preceding the @ unless there's a name
        //else throw
        public static string GetDisplayName(UserDO curUser)
        {
            string firstName = curUser.FirstName;
            string lastName = curUser.LastName;
            if (firstName != null)
            {
                if (lastName == null)
                    return firstName;

                return firstName + " " + lastName;
            }

            EmailAddressDO curEmailAddress = curUser.EmailAddress;
            if (curEmailAddress.Name != null)
                return curEmailAddress.Name;

            curEmailAddress.Address.ValidateEmailAddress();
            return curEmailAddress.Address.Split(new[] {'@'})[0];

            throw new ArgumentException("Failed to extract originator info from this Event. Something needs to be there.");
        }

        public List<UserDO> Query(IUnitOfWork uow, String userId)
        {
            List<UserDO> curUserList = new List<UserDO>();
            if (uow.UserRepository == null)
                return curUserList;

            curUserList = uow.UserRepository.GetAll().Where(x => (!string.IsNullOrEmpty(userId)) ? x.Id == userId : x.Id != null).ToList();

            return curUserList;
        }

    }
}
