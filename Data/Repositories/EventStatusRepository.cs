using Data.Entities;
using Data.Entities.Constants;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EventStatusRepository : GenericRepository<EventStatusRow>, IEventStatusRepository
    {
        public EventStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEventStatusRepository : IGenericRepository<EventStatusRow>
    {

    }
}


