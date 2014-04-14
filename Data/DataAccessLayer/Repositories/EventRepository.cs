using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class EventRepository : GenericRepository<EventDO>, IInvitationRepository
    {

        public EventRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IInvitationRepository : IGenericRepository<EventDO>
    {

    }
}
