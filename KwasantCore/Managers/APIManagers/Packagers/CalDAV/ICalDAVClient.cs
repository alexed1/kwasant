using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantICS.DDay.iCal;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    public interface ICalDAVClient
    {
        Task<IEnumerable<iCalendar>> GetEventsAsync(IRemoteCalendarLink calendarLink, DateTimeOffset @from, DateTimeOffset to);
        Task CreateEventAsync(IRemoteCalendarLink calendarLink, iCalendar calendarEvent);
    }
}