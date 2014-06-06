using System;
using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Infrastructure
{
    public class KwasantUserStore : UserStore<UserDO>, IKwasantUserStore
    {
        public IUserStore<UserDO> SetUnitOfWork(IUnitOfWork uow)
        {
            if (uow.Db is DbContext)
                return new UserStore<UserDO>(uow.Db as DbContext);
            throw new Exception("Invalid mocking setup.");
        }
    }
}
