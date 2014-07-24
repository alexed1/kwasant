using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EventStatusRepository : GenericRepository<EventStatus>, IEventStatusRepository
    {
        public EventStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEventStatusRepository : IGenericRepository<EventStatus>
    {

    }
}


