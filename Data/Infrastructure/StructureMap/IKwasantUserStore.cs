using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;

namespace Data.Infrastructure.StructureMap
{
    public interface IKwasantUserStore : IUserStore<UserDO>
    {
        IUserStore<UserDO> SetUnitOfWork(IUnitOfWork uow);
    }
}
