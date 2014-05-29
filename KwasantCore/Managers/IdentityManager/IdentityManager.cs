using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using Data.Interfaces;
using Data.Entities;
using Utilities;
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
        IUnitOfWork _uow;

        public IdentityManager(IUnitOfWork uow)
        {
            _user = new User(uow);
            _uow = uow;
        }

        //converts an "implicit" user, which is created when someone emails a request, to an "explict" user who has actually registered
        public void AttachPassword(UserDO userDO)
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
