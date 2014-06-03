using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Infrastructure.StructureMap
{
    public interface IKwasantRoleStore : IRoleStore<IdentityRole, string>
    {
        IRoleStore<IdentityRole, string> SetUnitOfWork(IUnitOfWork uow);
    }
}
