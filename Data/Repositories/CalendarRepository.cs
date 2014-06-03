using Data.Infrastructure;
using Data.Interfaces;
using Data.Entities;
using Data.Validators;
using FluentValidation;

namespace Data.Repositories
{
    public class CalendarRepository : GenericRepository<CalendarDO>, ICalendarRepository
    {
        private readonly CalendarValidator _curValidator;
        internal CalendarRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new CalendarValidator();
        }

        public new void Add(CalendarDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }


    public interface ICalendarRepository : IGenericRepository<CalendarDO>
    {

    }
}