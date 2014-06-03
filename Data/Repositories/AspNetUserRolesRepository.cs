using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetUserRolesRepository : GenericRepository<AspNetUserRolesDO>, IAspNetUserRolesRepository
    {

        internal AspNetUserRolesRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }

    public interface IAspNetUserRolesRepository : IGenericRepository<AspNetUserRolesDO>
    {
      
    }
}
