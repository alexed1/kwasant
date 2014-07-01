using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestStatusRepository : GenericRepository<BookingRequestStatusDO>, IBookingRequestStatusRepository
    {
        public BookingRequestStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IBookingRequestStatusRepository : IGenericRepository<BookingRequestStatusDO>
    {

    }
}


