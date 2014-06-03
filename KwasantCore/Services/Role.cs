using System.Collections.Generic;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using AutoMapper;
using StructureMap;

namespace KwasantCore.Services
{
    public class Role
    {
        public void Add(IUnitOfWork uow, AspNetRolesDO aspNetRolesDO)
        {
            var roleStore = new RoleStore<IdentityRole>(uow.Db);
            var currRole = new RoleManager<IdentityRole>(roleStore);
            currRole.Create(aspNetRolesDO);
        }

        public List<RoleData> GetRoles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<RoleData> currRoleDataList = new List<RoleData>();

                Mapper.CreateMap<IdentityRole, RoleData>();

                var currRole = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(uow.Db));

                foreach (IdentityRole role in currRole.Roles)
                {
                    RoleData roleData = Mapper.Map<IdentityRole, RoleData>(role);
                    currRoleDataList.Add(roleData);
                }

                return currRoleDataList;
            }
        }
    }
}
