using System;
using System.Linq;
using Data.Interfaces;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Collections.Generic;

using Data.Entities;

namespace Data.Repositories
{
    //This generic repository ensures minimum repetition in all of the other repositories.
    //The database context is injected via a UnitOfWork implementation
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CalendarRepository : ICalendarRepository
    {
        private readonly IUnitOfWork _unitOfWork;        
        public IDbSet<CalendarDO> dbSet;

        public CalendarRepository(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            _unitOfWork = unitOfWork;
            this.dbSet = _unitOfWork.Db.Set<CalendarDO>();
        }



        #region Property
        public IUnitOfWork UnitOfWork { get { return _unitOfWork; } }
        internal IDBContext Database { get { return _unitOfWork.Db; } }        

        #endregion

        #region Method

        public CalendarDO GetByKey(object keyValue)
        {
            return dbSet.Find(keyValue);
        }

        public IQueryable<CalendarDO> GetQuery()
        {
            return dbSet.AsEnumerable().AsQueryable<CalendarDO>();
        }


        public void Create(CalendarDO calendarDO)
        {
            dbSet.Add(calendarDO);
        }

        public void Delete(CalendarDO calendarDO)
        {
            dbSet.Remove(calendarDO);

        }

        public bool IsDetached(CalendarDO calendarDO)
        {
            if (calendarDO == null)
            {
                throw new ArgumentNullException("calendarDO");
            }
            return _unitOfWork.Db.Entry(calendarDO).State == EntityState.Detached;
        }

        public void Attach(CalendarDO calendarDO)
        {
            if (calendarDO == null)
            {
                throw new ArgumentNullException("calendarDO");
            }
            dbSet.Attach(calendarDO);
            _unitOfWork.Db.Entry(calendarDO).State = EntityState.Modified;
        }

        public IEnumerable<CalendarDO> GetAll()
        {
            
            return dbSet.AsEnumerable().ToList();
        }

        public virtual void Save(CalendarDO calendarDO)
        {

            _unitOfWork.Db.Entry(calendarDO).State = EntityState.Modified;


        }

        //http://stackoverflow.com/a/12587752/1915866
        public void Update(CalendarDO calendarDO, CalendarDO existingCalendarDO)
        {
            _unitOfWork.Db.Entry(existingCalendarDO).CurrentValues.SetValues(calendarDO);
        }

        public CalendarDO FindOne(Expression<Func<CalendarDO, bool>> criteria)
        {
            return dbSet.Where(criteria).FirstOrDefault();
        }

        public IEnumerable<CalendarDO> FindList(Expression<Func<CalendarDO, bool>> criteria)
        {
            return GetQuery().Where(criteria).ToList();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion



    }
}