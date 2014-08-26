using Data.Interfaces;
using Data.States.Templates;

namespace Data.Repositories
{
    public class BookingRequestStatusRepository : GenericRepository<_BookingRequestStateTemplate>, IBookingRequestStatusRepository
    {
        public BookingRequestStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IBookingRequestStatusRepository : IGenericRepository<_BookingRequestStateTemplate>
    {

    }
}


