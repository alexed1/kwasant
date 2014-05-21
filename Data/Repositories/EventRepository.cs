using System;
using System.Diagnostics;
using Data.Entities;
using Data.Interfaces;
using Data.Validators;
using FluentValidation;

namespace Data.Repositories
{


    public class EventRepository : GenericRepository<EventDO>, IEventRepository
    {
        private EventValidator _curValidator;

        internal EventRepository(IDBContext dbContext)
            : base(dbContext)
        {
            _curValidator = new EventValidator();
            
        }

        void IGenericRepository<EventDO>.Add(EventDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            Add(entity);
        }
    }


    public interface IEventRepository : IGenericRepository<EventDO>
    {

    }
}
