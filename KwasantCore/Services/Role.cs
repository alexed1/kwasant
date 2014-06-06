using System.Collections.Generic;
using Data.Infrastructure.StructureMap;
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
            var currRole = GetRoleManager(uow);
            currRole.Create(aspNetRolesDO);
        }

        public List<RoleData> GetRoles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<RoleData> currRoleDataList = new List<RoleData>();

                Mapper.CreateMap<IdentityRole, RoleData>();

                var currRole = GetRoleManager(uow);

                foreach (IdentityRole role in currRole.Roles)
                {
                    RoleData roleData = Mapper.Map<IdentityRole, RoleData>(role);
                    currRoleDataList.Add(roleData);
                }

                return currRoleDataList;
            }
        }

        public static RoleManager<IdentityRole> GetRoleManager(IUnitOfWork uow)
        {
            var roleStore = ObjectFactory.GetInstance<IKwasantRoleStore>();
            return new RoleManager<IdentityRole>(roleStore.SetUnitOfWork(uow));
        }
    }
}
