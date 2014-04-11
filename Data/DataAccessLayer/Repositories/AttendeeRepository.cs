using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class AttendeeRepository : GenericRepository<Attendee>, IAttendeeRepository
    {

        public AttendeeRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IAttendeeRepository : IGenericRepository<Attendee>
    {

    }
}
