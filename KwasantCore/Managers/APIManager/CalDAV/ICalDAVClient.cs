using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantICS.DDay.iCal;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public interface ICalDAVClient
    {
        Task<IEnumerable<iCalendar>> GetEventsAsync(ICalendar calendarInfo, DateTimeOffset from, DateTimeOffset to);
        Task CreateEventAsync(ICalendar calendarInfo, iCalendar calendarEvent);
    }
}