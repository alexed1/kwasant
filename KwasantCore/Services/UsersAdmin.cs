using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
//using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Web;
using Microsoft.Owin.Host.SystemWeb;
using System.Data.Entity;

using Data.Interfaces;
using Data.Entities;
using StructureMap;
using Data.Repositories;
using Utilities;
using Data.Infrastructure;
using KwasantCore;


namespace KwasantCore.Services
{
    public class UsersAdmin
    {
        private readonly IUnitOfWork _uow;
        private UserRepository _userRepo;        

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public UsersAdmin(IUnitOfWork uow)
        {
            _uow = uow;
            _userRepo = _uow.UserRepository;
        }

        public List<UsersAdminData> GetUsersAdminViewData(String userId, String roleId)
        {
            List<UsersAdminData> currUsersAdminDataList = new List<UsersAdminData>();

            if (_userRepo == null)
                return currUsersAdminDataList;             

            List<UserDO> currUserDOs = _userRepo.GetAll().ToList();

            var currUserManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));           

            if (currUserDOs != null)
            {
                foreach (UserDO userDO in currUserDOs)
                {
                    if (userDO.Roles != null && userDO.Roles.Count > 0)
                    {
                        if (userDO.Roles.ToList()[0].UserId == userId && userDO.Roles.ToList()[0].RoleId == roleId)
                        {
                            UsersAdminData currUsersAdminData = new UsersAdminData();

                            currUsersAdminData.UserId = userDO.Id;
                            currUsersAdminData.FirstName = userDO.FirstName;
                            currUsersAdminData.LasttName = userDO.LastName;
                            currUsersAdminData.EmailAddress = userDO.EmailAddress != null ? userDO.EmailAddress.Address : String.Empty;
                            currUsersAdminData.EmailAddressID = userDO.EmailAddress != null ? userDO.EmailAddress.Id : 0;
                            currUsersAdminData.RoleId = userDO.Roles != null && userDO.Roles.Count > 0 ? userDO.Roles.ToList()[0].RoleId : String.Empty;

                            if (currUserManager != null)
                            {
                                var roles = currUserManager.GetRoles(userDO.Id);
                                currUsersAdminData.Role = roles != null && roles.Count > 0 ? roles[0] : String.Empty;
                            }

                            currUsersAdminDataList.Add(currUsersAdminData);
                            break;
                        }
                    }
                }
            }

            return currUsersAdminDataList;
        }

        public List<UsersAdminData> GetUsersAdminViewData()
        {
            List<UsersAdminData> currUsersAdminDataList = new List<UsersAdminData>();
            List<UserDO> currUserDOs = _userRepo.GetAll().ToList();

            if (_userRepo == null)
                return currUsersAdminDataList;

            var currUserManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));

            if (currUserDOs != null)
            {
                foreach (UserDO userDO in currUserDOs)
                {
                    UsersAdminData currUsersAdminData = new UsersAdminData();

                    currUsersAdminData.UserId = userDO.Id;
                    currUsersAdminData.FirstName = userDO.FirstName;
                    currUsersAdminData.LasttName = userDO.LastName;
                    currUsersAdminData.EmailAddress = userDO.EmailAddress != null ? userDO.EmailAddress.Address : String.Empty;
                    currUsersAdminData.EmailAddressID = userDO.EmailAddress != null ? userDO.EmailAddress.Id : 0;
                    currUsersAdminData.RoleId = userDO.Roles != null && userDO.Roles.Count > 0 ? userDO.Roles.ToList()[0].RoleId : String.Empty;

                    if (currUserManager != null)
                    {
                        var roles = currUserManager.GetRoles(userDO.Id);
                        currUsersAdminData.Role = roles != null && roles.Count > 0 ? roles[0] : String.Empty;
                    }

                    currUsersAdminDataList.Add(currUsersAdminData);
                }
            }

            return currUsersAdminDataList;
        }
    }
}
