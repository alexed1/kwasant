using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantICS.DDay.iCal;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar
{
    public interface IRemoteCalendarServiceClient
    {
        Task<IEnumerable<IEvent>> GetEventsAsync(IRemoteCalendarLinkDO calendarLink, DateTimeOffset @from, DateTimeOffset to);
        Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, IEvent calendarEvent);
        Task<IDictionary<string, string>> GetCalendarsAsync(IRemoteCalendarAuthDataDO authData);
    }
}