using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{


    public class CommunicationConfigurationRepository : GenericRepository<CommunicationConfigurationDO>, ICommunicationConfigurationRepository
    {
        internal CommunicationConfigurationRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface ICommunicationConfigurationRepository : IGenericRepository<CommunicationConfigurationDO>
    {

    }
}
