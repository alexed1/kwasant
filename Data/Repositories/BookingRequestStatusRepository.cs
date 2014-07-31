using Data.Entities;
using Data.Entities.Constants;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Repositories
{
    public class BookingRequestStatusRepository : GenericRepository<BookingRequestStateRow>, IBookingRequestStatusRepository
    {
        public BookingRequestStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IBookingRequestStatusRepository : IGenericRepository<BookingRequestStateRow>
    {

    }
}


