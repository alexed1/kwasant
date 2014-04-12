using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class InvitationRepository : GenericRepository<EventDO>, IInvitationRepository
    {

        public InvitationRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IInvitationRepository : IGenericRepository<EventDO>
    {

    }
}
