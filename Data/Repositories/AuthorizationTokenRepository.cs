using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AuthorizationTokenRepository : GenericRepository<AuthorizationTokenDO>, IAuthorizationTokenRepository
    {
        internal AuthorizationTokenRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IAuthorizationTokenRepository : IGenericRepository<AuthorizationTokenDO>
    {

    }
}
