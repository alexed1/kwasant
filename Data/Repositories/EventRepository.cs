using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class EventRepository : GenericRepository<EventDO>, IEventRepository
    {

        public EventRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IEventRepository : IGenericRepository<EventDO>
    {

    }
}
