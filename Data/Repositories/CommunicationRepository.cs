using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class CommunicationConfigurationRepository : GenericRepository<CommunicationConfigurationDO>, ICommunicationConfigurationRepository
    {
        internal CommunicationConfigurationRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface ICommunicationConfigurationRepository : IGenericRepository<CommunicationConfigurationDO>
    {

    }
}
