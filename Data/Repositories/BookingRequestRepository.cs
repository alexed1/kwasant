using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestRepository : GenericRepository<BookingRequestDO>, IBookingRequestRepository
    {

        public BookingRequestRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IBookingRequestRepository : IGenericRepository<BookingRequestDO>
    {

    }
}
