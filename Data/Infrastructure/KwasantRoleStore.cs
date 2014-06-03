using System;
using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Infrastructure
{
    public class KwasantRoleStore : RoleStore<AspNetRolesDO>, IKwasantRoleStore
    {
        public IRoleStore<AspNetRolesDO, string> SetUnitOfWork(IUnitOfWork uow)
        {
            if (uow.Db is DbContext)
                return new RoleStore<AspNetRolesDO>(uow.Db as DbContext);
            throw new Exception("Invalid mocking setup.");
        }
    }
}
