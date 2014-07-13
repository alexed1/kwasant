using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Authorizers.Google;
using KwasantCore.Managers.APIManager.CalDAV.Google;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    class CalDAVClient : ICalDAVClient
    {
        private readonly ICalDAVEndPoint _endPoint;
        private readonly IHttpChannel _channel;

        public CalDAVClient(ICalDAVEndPoint endPoint, IHttpChannel channel)
        {
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");
            if (channel == null)
                throw new ArgumentNullException("channel");
            _endPoint = endPoint;
            _channel = channel;
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

        #region Implementation of ICalDAVClient

        public async Task<IEnumerable<iCalendar>> GetEventsAsync(ICalendar calendarInfo, DateTimeOffset @from, DateTimeOffset to)
        {
            if (calendarInfo == null)
                throw new ArgumentNullException("calendarInfo");

            var calendarId = calendarInfo.Owner.EmailAddress.Address;
            MultiStatus multiStatus;

            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("REPORT"), string.Format(_endPoint.EventsUrlFormat, calendarId));
                request.Headers.Add("Depth", "1");
                request.Content = new StringContent(string.Format(EventsQueryFormat, @from.UtcDateTime, to.UtcDateTime), Encoding.UTF8, "application/xml");
                return request;
            };

            using (var response = await _channel.SendRequestAsync(requestFactoryMethod))
            {
                using (var xmlStream = await response.Content.ReadAsStreamAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MultiStatus));
                    multiStatus = (MultiStatus)serializer.Deserialize(xmlStream);
                }
            }

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

        public async Task CreateEventAsync(ICalendar calendarInfo, iCalendar calendarEvent)
        {
            if (calendarInfo == null)
                throw new ArgumentNullException("calendarInfo");
            if (calendarEvent == null)
                throw new ArgumentNullException("calendarEvent");
            if (calendarEvent.Events == null || calendarEvent.Events.Count == 0)
                throw new ArgumentException("iCalendar object must contain at least one event.", "calendarEvent");

            var calendarId = calendarInfo.Owner.EmailAddress.Address;
            var eventId = calendarEvent.Events.First().UID;

            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, string.Format(_endPoint.EventUrlFormat, calendarId, eventId));
                request.Headers.Add("If-None-Match", "*");
                iCalendarSerializer serializer = new iCalendarSerializer(calendarEvent);
                string calendarString = serializer.Serialize(calendarEvent);
                request.Content = new StringContent(calendarString, Encoding.UTF8, "text/calendar");
                return request;
            };

            using (var response = await _channel.SendRequestAsync(requestFactoryMethod))
            {
            }
        }

        #endregion
    }
}