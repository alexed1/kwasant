using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetUserRolesRepository : GenericRepository<AspNetUserRolesDO>, IAspNetUserRolesRepository
    {

        internal AspNetUserRolesRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {
            
        }
    }

    public interface IAspNetUserRolesRepository : IGenericRepository<AspNetUserRolesDO>
    {
      
    }
}
