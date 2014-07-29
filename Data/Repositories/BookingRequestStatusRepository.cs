using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestStatusRepository : GenericRepository<BookingRequestState>, IBookingRequestStatusRepository
    {
        public BookingRequestStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IBookingRequestStatusRepository : IGenericRepository<BookingRequestState>
    {

    }
}


