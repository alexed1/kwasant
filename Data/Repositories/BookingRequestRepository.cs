using Data.Entities;
using Data.Interfaces;
using Data.Validators;
using FluentValidation;

namespace Data.Repositories
{
    public class BookingRequestRepository : GenericRepository<BookingRequestDO>, IBookingRequestRepository
    {
<<<<<<< HEAD
        internal BookingRequestRepository(IDBContext dbContext)
            : base(dbContext)
=======

        private BookingRequestValidator _curValidator ;
        internal BookingRequestRepository(IUnitOfWork uow)
            : base(uow)
>>>>>>> v0.5rc7
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
