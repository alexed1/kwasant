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

        public string GetRole(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curManager = GetUserManager(uow);
                return curManager.GetRoles(curUserId).Any() ? curManager.GetRoles(curUserId)[0] : "";
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
        }
    }
}
