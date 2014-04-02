using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DDay.iCal;
using Shnexy.DDay.iCal.Serialization.iCalendar;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{

    //The EventRepository works differently from other repositories. It does not attempt to persist the Event class directly via EF but instead serializes it and stores the resulting
    //ics text as an EventFile body.
    public class EventRepository : GenericRepository<Event>,  IEventRepository
    {
        internal DbContext Database { get { return _unitOfWork.Db; } }
        private readonly IUnitOfWork _unitOfWork;
        internal DbSet<EventFile> dbSet;
        public EventRepository(IUnitOfWork uow) : base(uow)
        {
            
        }



        public void Add(Event curEvent)
        {
            //serialize the Event
    
            iCalendar iCal = new iCalendar();
            iCal.AddChild(curEvent);
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string eventBody = serializer.Serialize(iCal);

            //create a new EventFile class
            //populate its body
            var curEventFile = new EventFile();
            curEventFile.Body = eventBody;
            
            //save the EventFile.
            dbSet.Add(curEventFile);
            //do we then know the id?
            Debug.WriteLine("id is " + curEventFile.Id);
        }





          public Event GetByKey(object keyValue)
        {
            return dbSet.Find(keyValue);
        }

        public IQueryable<Event> GetQuery()
        {
            return dbSet.AsEnumerable().AsQueryable<Event>();
        }




        public void Remove(Event entity)
        {
            dbSet.Remove(entity);

        }

        public void Attach(Event entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            dbSet.Attach(entity);
            _unitOfWork.Db.Entry(entity).State = EntityState.Modified;
        }

        public IEnumerable<Event> GetAll()
        {
            
            return dbSet.AsEnumerable().ToList();
        }

        public virtual void Save(Event entity)
        {

            _unitOfWork.Db.Entry(entity).State = EntityState.Modified;


        }
        //http://stackoverflow.com/a/12587752/1915866
        public virtual void Update(Event entity, Event existingEntity)
        {

            _unitOfWork.Db.Entry(existingEntity).CurrentValues.SetValues(entity);

        }

        public Event FindOne(Expression<Func<Event, bool>> criteria)
        {
            return dbSet.Where(criteria).FirstOrDefault();
        }

        public IEnumerable<Event> FindList(Expression<Func<Event, bool>> criteria)
        {
            return GetQuery().Where(criteria).ToList();
        }

  

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion


    }


    public interface IEventRepository : IGenericRepository<Event>
    {
        IUnitOfWork UnitOfWork { get; }

        
   
    }
}
