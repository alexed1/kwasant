using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class AttendeeRepository : GenericRepository<AttendeeDO>, IAttendeeRepository
    {
        internal AttendeeRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IAttendeeRepository : IGenericRepository<AttendeeDO>
    {

    }
}
