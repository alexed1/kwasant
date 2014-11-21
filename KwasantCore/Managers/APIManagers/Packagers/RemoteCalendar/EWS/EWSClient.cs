using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantICS.DDay.iCal;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar.EWS
{
    class EWSClient : IRemoteCalendarServiceClient
    {
        public Task<IEnumerable<iCalendar>> GetEventsAsync(IRemoteCalendarLinkDO calendarLink, DateTimeOffset @from, DateTimeOffset to)
        {
            throw new NotImplementedException();
        }

        public Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, iCalendar calendarEvent)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetCalendarsAsync(IRemoteCalendarAuthDataDO authData)
        {
            throw new NotImplementedException();
        }
    }
}
