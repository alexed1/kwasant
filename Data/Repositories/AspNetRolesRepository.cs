using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetRolesRepository : GenericRepository<AspNetRolesDO>, IAspNetRolesRepository
    {

        internal AspNetRolesRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }
    }

    public interface IAspNetRolesRepository : IGenericRepository<AspNetRolesDO>
    {
      
    }
}
