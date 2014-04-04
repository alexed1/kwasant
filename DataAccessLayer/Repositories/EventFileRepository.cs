using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{

    public class EventFileRepository : GenericRepository<EventFile>, IEventFileRepository
    {

        public EventFileRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEventFileRepository
    {
        IUnitOfWork UnitOfWork { get; }
        EventFile GetByKey(object keyValue);
        IQueryable<EventFile> GetQuery();
        void Add(EventFile entity);
        void Remove(EventFile entity);
        void Attach(EventFile entity);
        IEnumerable<EventFile> GetAll();
        void Save(EventFile entity);
        void Update(EventFile entity, EventFile existingEntity);
        EventFile FindOne(Expression<Func<EventFile, bool>> criteria);
        IEnumerable<EventFile> FindList(Expression<Func<EventFile, bool>> criteria);
        void Dispose();
    }
}
