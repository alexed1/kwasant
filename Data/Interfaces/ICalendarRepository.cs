using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

using Data.Entities;

namespace Data.Interfaces
{
    public interface ICalendarRepository
    {
        IUnitOfWork UnitOfWork { get; }
        CalendarDO GetByKey(object keyValue);
        IQueryable<CalendarDO> GetQuery();
        void Create(CalendarDO calendarDO);
        void Delete(CalendarDO calendarDO);
        void Attach(CalendarDO calendarDO);
        IEnumerable<CalendarDO> GetAll();
        void Save(CalendarDO calendarDO);
        void Update(CalendarDO calendarDO, CalendarDO existingCalendarDO);
        CalendarDO FindOne(Expression<Func<CalendarDO, bool>> criteria);
        IEnumerable<CalendarDO> FindList(Expression<Func<CalendarDO, bool>> criteria);
        void Dispose();
    }
}