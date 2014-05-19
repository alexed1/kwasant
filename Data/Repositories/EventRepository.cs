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

        public EventRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new EventValidator();
            
        }


        public void Add(EventDO entity)
        {
 
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }


    public interface IEventRepository : IGenericRepository<EventDO>
    {

    }
}
