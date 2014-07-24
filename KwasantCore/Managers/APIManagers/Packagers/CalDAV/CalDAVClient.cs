using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Data.Interfaces;
using KwasantCore.Managers.APIManagers.Transmitters.Http;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    /// <summary>
    /// CalDAV requests packager
    /// </summary>
    class CalDAVClient : ICalDAVClient
    {
        private readonly IAuthorizingHttpChannel _channel;

        private readonly string _endPoint;
        private readonly string _calendarBaseUrlFormat;
        private readonly string _eventsUrlFormat;
        private readonly string _eventUrlFormat;

        public CalDAVClient(string endPoint, IAuthorizingHttpChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            if (string.IsNullOrEmpty(endPoint))
                throw new ArgumentException("CalDAV endpoint cannot be empty or null.", "endPoint");
            _channel = channel;
            _endPoint = endPoint;

            _calendarBaseUrlFormat = string.Concat(_endPoint, "/{0}");
            _eventsUrlFormat = string.Concat(_calendarBaseUrlFormat, "/events");
            _eventUrlFormat = string.Concat(_eventsUrlFormat, "/{1}");
        }

        private const string EventsQueryFormat =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
            "<C:calendar-query xmlns:D=\"DAV:\" xmlns:C=\"urn:ietf:params:xml:ns:caldav\">\r\n" +
            "<D:prop>\r\n" +
            "<D:getetag/>\r\n" +
            "<C:calendar-data/>\r\n" +
            "</D:prop>\r\n" +
            "<C:filter>\r\n" +
            "<C:comp-filter name=\"VCALENDAR\">\r\n" +
            "<C:comp-filter name=\"VEVENT\">\r\n" +
            "<C:time-range start=\"{0:yyyyMMddTHHmmssZ}\" end=\"{1:yyyyMMddTHHmmssZ}\"/>\r\n" +
            "</C:comp-filter>\r\n" +
            "</C:comp-filter>\r\n" +
            "</C:filter>\r\n" +
            "</C:calendar-query>";

        /// <summary>
        /// Creates a request to CalDAV service for retrieving all calendar events in range of from..to. 
        /// See CalDAV reference for more details.
        /// </summary>
        /// <param name="calendarLink"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<IEnumerable<iCalendar>> GetEventsAsync(IRemoteCalendarLink calendarLink, DateTimeOffset @from, DateTimeOffset to)
        {
            if (calendarLink == null)
                throw new ArgumentNullException("calendarLink");

            var calendarId = calendarLink.RemoteCalendarName;
            var userId = calendarLink.LocalCalendar.Owner.Id;
            
            // Standard structure to get responses from CalDAV (WebDAV) services.
            MultiStatus multiStatus;

            // We need factory method rather than just an instance here as it is required by IHttpChannel.SendRequestAsync. 
            // See that method documentation for more details.
            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("REPORT"), string.Format(_eventsUrlFormat, calendarId));
                request.Headers.Add("Depth", "1");
                request.Content = new StringContent(string.Format(EventsQueryFormat, @from.UtcDateTime, to.UtcDateTime), Encoding.UTF8, "application/xml");
                return request;
            };

            using (var response = await _channel.SendRequestAsync(requestFactoryMethod, userId))
            {
                using (var xmlStream = await response.Content.ReadAsStreamAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MultiStatus));
                    multiStatus = (MultiStatus)serializer.Deserialize(xmlStream);
                }
            }

            // extract ICS objects from response
            return multiStatus.Items != null 
                ? multiStatus.Items
                    .Select(r =>
                            {
                                using (var stringReader = new StringReader(r.PropStat.First().Prop.CalendarData))
                                {
                                    return (iCalendar) iCalendar.LoadFromStream(stringReader)[0];
                                }
                            })
                    .ToArray() 
                : new iCalendar[0];
        }

        public async Task CreateEventAsync(IRemoteCalendarLink calendarLink, iCalendar calendarEvent)
        {
            if (calendarLink == null)
                throw new ArgumentNullException("calendarLink");
            if (calendarEvent == null)
                throw new ArgumentNullException("calendarEvent");
            if (calendarEvent.Events == null || calendarEvent.Events.Count == 0)
                throw new ArgumentException("iCalendar object must contain at least one event.", "calendarEvent");

            var calendarId = calendarLink.RemoteCalendarName;
            var userId = calendarLink.LocalCalendar.Owner.Id;
            var eventId = calendarEvent.Events.First().UID;

            // We need factory method rather than just an instance here as it is required by IHttpChannel.SendRequestAsync. 
            // See that method documentation for more details.
            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, string.Format(_eventUrlFormat, calendarId, eventId));
                request.Headers.Add("If-None-Match", "*");
                iCalendarSerializer serializer = new iCalendarSerializer(calendarEvent);
                string calendarString = serializer.Serialize(calendarEvent);
                request.Content = new StringContent(calendarString, Encoding.UTF8, "text/calendar");
                return request;
            };

            using (var response = await _channel.SendRequestAsync(requestFactoryMethod, userId))
            {
            }
        }

    }
}