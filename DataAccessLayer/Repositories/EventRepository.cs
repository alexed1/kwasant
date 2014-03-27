using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{

    public class EventRepository : GenericRepository<Event>, IEventRepository
    {

        public EventRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }

    public interface IEventRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Event GetByKey(object keyValue);
        IQueryable<Event> GetQuery();
        void Add(Event entity);
        void Remove(Event entity);
        void Attach(Event entity);
        IEnumerable<Event> GetAll();
        void Save(Event entity);
        void Update(Event entity, Event existingEntity);
        Event FindOne(Expression<Func<Event, bool>> criteria);
        IEnumerable<Event> FindList(Expression<Func<Event, bool>> criteria);
        void Dispose();
    }
}
