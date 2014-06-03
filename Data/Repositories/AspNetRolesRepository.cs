using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetRolesRepository : GenericRepository<AspNetRolesDO>, IAspNetRolesRepository
    {

        internal AspNetRolesRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {
            
        }
    }

    public interface IAspNetRolesRepository : IGenericRepository<AspNetRolesDO>
    {
      
    }
}
