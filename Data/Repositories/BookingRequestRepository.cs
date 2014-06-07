using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestRepository : GenericRepository<BookingRequestDO>, IBookingRequestRepository
    {
        internal BookingRequestRepository(IDBContext dbContext)
            : base(dbContext)
        {

        }
    }


    public interface IBookingRequestRepository : IGenericRepository<BookingRequestDO>
    {

    }
}
