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
using AutoMapper;

namespace KwasantCore.Services
{
    public class Role
    {
        private readonly IUnitOfWork _uow;

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public Role(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Add(AspNetRolesDO aspNetRolesDO)
        {   
                var currRole = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_uow.Db as KwasantDbContext));
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
