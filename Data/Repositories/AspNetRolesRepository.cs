using System.Linq;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Repositories
{
    public class AspNetRolesRepository : GenericRepository<IdentityRole>, IAspNetRolesRepository
    {

        internal AspNetRolesRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

    }

    public interface IAspNetRolesRepository : IGenericRepository<IdentityRole>
    {
        
    }
}
