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
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    class GoogleCalDAVClient : ICalDAVClient
    {
        private readonly GoogleCalendarAuthorizer _googleCalendarAuthorizer;

        public GoogleCalDAVClient(string userId)
        {
            _googleCalendarAuthorizer = new GoogleCalendarAuthorizer(userId);
        }

        private const string BaseUrl = "https://apidata.googleusercontent.com/caldav/v2";
        private const string CalendarBaseUrlFormat = BaseUrl + "/{0}";
        private const string EventsUrlFormat = CalendarBaseUrlFormat + "/events";
        private const string EventUrlFormat = EventsUrlFormat + "/{1}";

        private async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactoryMethod)
        {
            HttpResponseMessage response;
            using (HttpClient client = new HttpClient())
            {
                do
                {
                    using (var request = requestFactoryMethod())
                    {
                        var accessToken = await _googleCalendarAuthorizer.GetAccessTokenAsync(CancellationToken.None);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                await _googleCalendarAuthorizer.RefreshTokenAsync(CancellationToken.None);
                                response.Dispose();
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }
                    }
                } while (!response.IsSuccessStatusCode);
            }
            return response;
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
            var calendarId = calendarInfo.Owner.EmailAddress.Address;
            MultiStatus multiStatus;

            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("REPORT"), string.Format(EventsUrlFormat, calendarId));
                request.Headers.Add("Depth", "1");
                request.Content = new StringContent(string.Format(EventsQueryFormat, @from.UtcDateTime, to.UtcDateTime), Encoding.UTF8, "application/xml");
                return request;
            };

            using (var response = await SendRequestAsync(requestFactoryMethod))
            {
                using (var xmlStream = await response.Content.ReadAsStreamAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MultiStatus));
                    multiStatus = (MultiStatus)serializer.Deserialize(xmlStream);
                }
            }

            return multiStatus.Items
                .Select(r =>
                            {
                                using (var stringReader = new StringReader(r.PropStat.First().Prop.CalendarData))
                                {
                                    return (iCalendar) iCalendar.LoadFromStream(stringReader)[0];
                                }
                            })
                .ToArray();
        }

        public async Task CreateEventAsync(ICalendar calendarInfo, iCalendar calendarEvent)
        {
            var calendarId = calendarInfo.Owner.EmailAddress.Address;
            var eventId = calendarEvent.Events.First().UID;

            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, string.Format(EventUrlFormat, calendarId, eventId));
                request.Headers.Add("If-None-Match", "*");
                iCalendarSerializer serializer = new iCalendarSerializer(calendarEvent);
                string calendarString = serializer.Serialize(calendarEvent);
                request.Content = new StringContent(calendarString, Encoding.UTF8, "text/calendar");
                return request;
            };

            using (var response = await SendRequestAsync(requestFactoryMethod))
            {
            }
        }

        #endregion
    }
}