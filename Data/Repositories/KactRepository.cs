using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{


    public class KactRepository : GenericRepository<KactDO>, IKactRepository
    {
        internal KactRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IKactRepository : IGenericRepository<KactDO>
    {

    }
}
