using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure;
using AutoMapper;

namespace KwasantCore.Services
{
    public class Role
    {
        public void Add(AspNetRolesDO aspNetRolesDO)
        {
            DbContext context = _uow.Db as DbContext;
            var roleStore = new RoleStore<IdentityRole>(context);
            var currRole = new RoleManager<IdentityRole>(roleStore);
            currRole.Create(aspNetRolesDO);
        }

        public List<RoleData> GetRoles()
        {
            List<RoleData> currRoleDataList = new List<RoleData>();

            Mapper.CreateMap<IdentityRole, RoleData>();

            var currRole = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_uow.Db as KwasantDbContext));

            if (currRole != null)
            {
                foreach (IdentityRole role in currRole.Roles)
                {
                    RoleData roleData = Mapper.Map<IdentityRole, RoleData>(role);
                    currRoleDataList.Add(roleData);
                }
            }

            return currRoleDataList;
        }
    }
}
