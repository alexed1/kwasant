using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class RemoteCalendarProviderRepository : GenericRepository<RemoteCalendarProviderDO>, IRemoteCalendarProviderRepository
    {
        internal RemoteCalendarProviderRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteCalendarProviderDO GetByName(string name)
        {
            return GetQuery().FirstOrDefault(rcp => rcp.Name == name);
        }
    }

    public interface IRemoteCalendarProviderRepository : IGenericRepository<RemoteCalendarProviderDO>
    {
        RemoteCalendarProviderDO GetByName(string name);
    }
}
