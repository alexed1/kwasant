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

        public override void Add(EventDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }


    public interface IEventRepository : IGenericRepository<EventDO>
    {

    }
}
