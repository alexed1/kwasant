using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestStatusRepository : GenericRepository<BookingRequestDO>, IBookingRequestStatusRepository
    {
        public BookingRequestStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IBookingRequestStatusRepository : IGenericRepository<BookingRequestDO>
    {

    }
}


