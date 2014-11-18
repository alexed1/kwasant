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
            if (userDO.UserBookingRequests != null && userDO.UserBookingRequests.Any())
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

        public String[] GetAuthRolesList(string selectedRole) 
        {
            String[] userRoles = { };
            switch (selectedRole)
            {
                case "Admin":
                    userRoles = new[] { "Admin", "Booker", "Customer" };
                    break;
                case "Booker":
                    userRoles = new[] { "Booker", "Customer" };
                    break;
                case "Customer":
                    userRoles = new[] { "Customer" };
                    break;
            }
            return userRoles;
        }

        public String GetSelectedRole(String[] userRoles)
        {
            string slectedRoles = "";
            if (userRoles.Contains("Admin"))
                slectedRoles = "Admin";
            else if (userRoles.Contains("Booker"))
                slectedRoles = "Booker";
            else if (userRoles.Contains("Customer"))
                slectedRoles = "Customer";
            return slectedRoles;
        }

        public void SetRoles(IUnitOfWork uow, string userId, string selectedRole)
        {
            var existingRoles = uow.AspNetUserRolesRepository.GetRoles(userId).ToList();
            var seletedRoles = GetAuthRolesList(selectedRole);

            //Remove old roles
            foreach (var existingRole in existingRoles)
            {
                if (!seletedRoles.Select(newRole => newRole).Contains(existingRole.Name))
                    uow.AspNetUserRolesRepository.RevokeRoleFromUser(existingRole.Name, userId);
            }

            //Add new roles
            foreach (var role in seletedRoles)
            {
                if (!existingRoles.Select(newRole => newRole.Name).Contains(role))
                    uow.AspNetUserRolesRepository.AssignRoleToUser(role, userId);
            }
        }

        public UserDO GetUserToAddOrUpdate(IUnitOfWork uow, string userId, string emailAddress, string firstName, out bool isAlreadyExists)
        {
            UserDO existingUser;
            isAlreadyExists = false;
            if (!String.IsNullOrWhiteSpace(userId))
                existingUser = uow.UserRepository.GetByKey(userId);
            else
            {
                existingUser = uow.UserRepository.GetQuery().Where(e => e.EmailAddress.Address == emailAddress).FirstOrDefault();
                if (existingUser == null)
                {
                    existingUser = uow.UserRepository.GetOrCreateUser(emailAddress);
                }
                else
                {
                    isAlreadyExists = true;
                }
            }
            existingUser.EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress, firstName);
            return existingUser;
        }
    }
}
