using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using Data.Interfaces;
using Data.Entities;
using UtilitiesLib;
using StructureMap;
using KwasantCore.Services;

namespace KwasantCore.Managers.IdentityManager
{
    /// <summary>
    /// Helper class for user registartion and authentication.
    /// </summary>
    public class IdentityManager
    {
        private User _user;
        private Person _person;
        IUnitOfWork _uow;

        public IdentityManager(IUnitOfWork uow)
        {
            _user = new User(uow);
            _person = new Person(uow);
            _uow = uow;
        }

  
        public async Task<UserDO> ConvertExistingPerson(PersonDO personDO, UserDO userRegStrings)
        {
            UserDO newUserDO = new UserDO
            {
                UserName = userRegStrings.UserName,
                PersonDO = userRegStrings.PersonDO,
                Password = userRegStrings.Password
            };
             RegistrationStatus curRegStatus = await _user.Create(newUserDO, "Customer");
             if (curRegStatus == RegistrationStatus.Successful)
             {
                 _person.Delete(personDO);
             }

             return newUserDO;
        }

        public void UpdatePassword(UserDO userDO)
        {
            if (userDO != null)
            {
                _user.UpdatePassword(userDO);
            }
        }

        public async Task<RegistrationStatus> RegisterNewUser(UserDO userDO)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Pending;
            
            curRegStatus = await _user.Create(userDO, "Customer");

            return curRegStatus;
        }

        public async Task<LoginStatus> Login(UserDO userDO, bool isPersistent)
        {
            return await _user.Login(userDO, isPersistent);
        }

        public void LogOff()
        {
            _user.LogOff();
        }
    }
}
