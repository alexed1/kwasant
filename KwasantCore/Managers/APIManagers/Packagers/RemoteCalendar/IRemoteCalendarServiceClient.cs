using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantICS.DDay.iCal;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar
{
    public interface IRemoteCalendarServiceClient
    {
        Task<IEnumerable<iCalendar>> GetEventsAsync(IRemoteCalendarLinkDO calendarLink, DateTimeOffset @from, DateTimeOffset to);
        Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, iCalendar calendarEvent);
        Task<IDictionary<string, string>> GetCalendarsAsync(IRemoteCalendarAuthDataDO authData);
    }
}