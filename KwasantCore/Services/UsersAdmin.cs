using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Interfaces;
using Data.Entities;
using StructureMap;
using Data.Infrastructure;


namespace KwasantCore.Services
{
    public class UsersAdmin
    {
        public List<UsersAdminData> GetUsersAdminViewData(String userId, String roleId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<UsersAdminData> currUsersAdminDataList = new List<UsersAdminData>();

                if (uow.UserRepository == null)
                    return currUsersAdminDataList;

                List<UserDO> currUserDOs = uow.UserRepository.GetAll().ToList();

                var currUserManager = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));

                foreach (UserDO userDO in currUserDOs)
                {
                    if (userDO.Roles != null && userDO.Roles.Count > 0)
                    {
                        if (userDO.Roles.ToList()[0].UserId == userId && userDO.Roles.ToList()[0].RoleId == roleId)
                        {
                            UsersAdminData currUsersAdminData = new UsersAdminData();

                            currUsersAdminData.UserId = userDO.Id;
                            currUsersAdminData.FirstName = userDO.FirstName;
                            currUsersAdminData.LastName = userDO.LastName;
                            currUsersAdminData.EmailAddress = userDO.EmailAddress != null
                                ? userDO.EmailAddress.Address
                                : String.Empty;
                            currUsersAdminData.EmailAddressID = userDO.EmailAddress != null
                                ? userDO.EmailAddress.Id
                                : 0;
                            currUsersAdminData.RoleId = userDO.Roles != null && userDO.Roles.Count > 0
                                ? userDO.Roles.ToList()[0].RoleId
                                : String.Empty;

                            var roles = currUserManager.GetRoles(userDO.Id);
                            currUsersAdminData.Role = roles != null && roles.Count > 0 ? roles[0] : String.Empty;

                            currUsersAdminDataList.Add(currUsersAdminData);
                            break;
                        }
                    }
                }

                return currUsersAdminDataList;
            }
        }

        public List<UsersAdminData> GetUsersAdminViewData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<UsersAdminData> currUsersAdminDataList = new List<UsersAdminData>();
                List<UserDO> currUserDOs = uow.UserRepository.GetAll().ToList();

                var currUserManager = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));

                foreach (UserDO userDO in currUserDOs)
                {
                    UsersAdminData currUsersAdminData = new UsersAdminData();

                    currUsersAdminData.UserId = userDO.Id;
                    currUsersAdminData.FirstName = userDO.FirstName;
                    currUsersAdminData.LastName = userDO.LastName;
                    currUsersAdminData.EmailAddress = userDO.EmailAddress != null
                        ? userDO.EmailAddress.Address
                        : String.Empty;
                    currUsersAdminData.EmailAddressID = userDO.EmailAddress != null ? userDO.EmailAddress.Id : 0;
                    currUsersAdminData.RoleId = userDO.Roles != null && userDO.Roles.Count > 0
                        ? userDO.Roles.ToList()[0].RoleId
                        : String.Empty;

                    var roles = currUserManager.GetRoles(userDO.Id);
                    currUsersAdminData.Role = roles != null && roles.Count > 0 ? roles[0] : String.Empty;

                    currUsersAdminDataList.Add(currUsersAdminData);
                }

                return currUsersAdminDataList;
            }
        }
    }
}
