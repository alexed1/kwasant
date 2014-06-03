using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestRepository : GenericRepository<BookingRequestDO>, IBookingRequestRepository
    {
        internal BookingRequestRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }
    }


    public interface IBookingRequestRepository : IGenericRepository<BookingRequestDO>
    {

    }
}
