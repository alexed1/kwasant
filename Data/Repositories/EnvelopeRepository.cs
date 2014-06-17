using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EnvelopeRepository : GenericRepository<EnvelopeDO>
    {
        internal EnvelopeRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }

    public interface IEnvelopeRepository : IGenericRepository<EnvelopeDO>
    {
        
    }

}
