using System;
using System.Linq;
using Data.Interfaces;
using Data.Validations;
using Data.Entities;
using Data.States;
using StructureMap;

namespace KwasantCore.Services
{
    public class User
    {
               
        public void UpdatePassword(IUnitOfWork uow, UserDO userDO, string password)
        {
            if (userDO != null)
            {
                uow.UserRepository.UpdateUserCredentials(userDO, password: password);
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

        //
        //get roles for this User
        //if at least one role meets or exceeds the provided level, return true, else false
        public bool VerifyMinimumRole(string minAuthLevel, string curUserId, IUnitOfWork uow)
        {
            var roleIds = uow.AspNetUserRolesRepository.GetQuery().Where(ur => ur.UserId == curUserId).Select(ur => ur.RoleId).ToList();
            var roles = uow.AspNetRolesRepository.GetQuery().Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();

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

        public void Create(IUnitOfWork uow, UserDO submittedUserData, string role, bool sendEmail)
        {
            if (sendEmail)
            {
                new Email().SendUserSettingsNotification(uow, submittedUserData);
            }
            new Account().Register(uow, submittedUserData.EmailAddress.Address, submittedUserData.FirstName, submittedUserData.LastName, "test@1234", role);
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
