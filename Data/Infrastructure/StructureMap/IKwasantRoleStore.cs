using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Infrastructure.StructureMap
{
    public interface IKwasantRoleStore : IRoleStore<AspNetRolesDO, string>
    {
        IRoleStore<AspNetRolesDO, string> SetUnitOfWork(IUnitOfWork uow);
    }
}
