using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class AttendeeRepository : GenericRepository<AttendeeDO>, IAttendeeRepository
    {

        public AttendeeRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IAttendeeRepository : IGenericRepository<AttendeeDO>
    {

    }
}
