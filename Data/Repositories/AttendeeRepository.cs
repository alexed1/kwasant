using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{


    public class AttendeeRepository : GenericRepository<AttendeeDO>, IAttendeeRepository
    {
        internal AttendeeRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface IAttendeeRepository : IGenericRepository<AttendeeDO>
    {

    }
}
