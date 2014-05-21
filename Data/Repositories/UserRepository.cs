using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<UserDO>, IUserRepository
    {
        internal UserRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }
    }
}