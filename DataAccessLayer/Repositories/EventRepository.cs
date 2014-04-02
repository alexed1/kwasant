using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
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
    //ics text as an EventFile body. So everything here has to be special cased from the generic repository to use EventFile type instead of Event.

    //The serialization logic assumes that an ICS file contains a collection of calendards, each of which contains a collection of events. However, we'll always only have one event in one calendar in the strings that are read and written here.
    public class EventRepository : GenericRepository<Event>,  IEventRepository
    {
        internal DbContext Database { get { return _unitOfWork.Db; } }
        private readonly IUnitOfWork _unitOfWork;
        internal DbSet<EventFile> dbSet;
        public EventRepository(IUnitOfWork uow) : base(uow)
        {
             if (uow == null) throw new ArgumentNullException("unitOfWork");
            _unitOfWork = uow;
            this.dbSet = _unitOfWork.Db.Set<EventFile>();
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

            //add the attendees, which are currently stored separately
            curEventFile.Attendees = curEvent.Attendees;
            
            //save the EventFile.
            dbSet.Add(curEventFile);
            //do we then know the id?
            Debug.WriteLine("id is " + curEventFile.Id);
        }





          public Event GetByKey(object keyValue)
        {
            EventFile curEventFile =  dbSet.Find(keyValue);
            iCalendar iCal = new iCalendar();
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            StringReader reader = new StringReader(curEventFile.Body);
            iCalendarCollection curCollection  = (iCalendarCollection)serializer.Deserialize(reader);
            iCal = (iCalendar)curCollection.First();
            Event curEvent = (Event)iCal.Events.First();

            //add the attendees, which are currently stored separately
            curEvent.Attendees = curEventFile.Attendees.ToList();
            return curEvent;
        }

        




        public void Remove(Event entity)
        {
            //dbSet.Remove(entity);

        }

        public void Attach(Event entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

           // dbSet.Attach(entity);
          ///  _unitOfWork.Db.Entry(entity).State = EntityState.Modified;
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
