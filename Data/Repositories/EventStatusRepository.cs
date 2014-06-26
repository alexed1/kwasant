using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EventStatusRepository : GenericRepository<EventStatusDO>, IEventStatusRepository
    {
        public EventStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEventStatusRepository : IGenericRepository<EventStatusDO>
    {

    }
}


