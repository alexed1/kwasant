using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ClarificationRequestRepository : GenericRepository<ClarificationRequestDO>, IClarificationRequestRepository
    {
        internal ClarificationRequestRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }

    public interface IClarificationRequestRepository : IGenericRepository<ClarificationRequestDO>
    {
    }
}
