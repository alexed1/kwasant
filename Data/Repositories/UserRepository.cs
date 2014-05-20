using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<UserDO>, IUserRepository
    {
        public UserRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }
}