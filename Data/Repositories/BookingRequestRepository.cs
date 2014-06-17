using Data.Entities;
using Data.Interfaces;
using Data.Validators;

namespace Data.Repositories
{
    public class BookingRequestRepository : GenericRepository<BookingRequestDO>, IBookingRequestRepository
    {

        private BookingRequestValidator _curValidator ;
        internal BookingRequestRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new BookingRequestValidator();
        }

        public override void Add(BookingRequestDO entity)
        {
           // _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }


    public interface IBookingRequestRepository : IGenericRepository<BookingRequestDO>
    {

    }
}
