using System;
using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Infrastructure
{
    public class KwasantRoleStore : RoleStore<IdentityRole>, IKwasantRoleStore
    {
        public IRoleStore<IdentityRole, string> SetUnitOfWork(IUnitOfWork uow)
        {
            if (uow.Db is DbContext)
                return new RoleStore<IdentityRole>(uow.Db as DbContext);
            throw new Exception("Invalid mocking setup.");
        }
    }
}
