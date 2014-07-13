using KwasantCore.Managers.APIManager.Authorizers;
using KwasantCore.Managers.APIManager.Authorizers.Google;

namespace KwasantCore.Managers.APIManager.CalDAV.Google
{
    internal class GoogleCalDAVEndPoint : ICalDAVEndPoint
    {
        private const string BaseUrl = "https://apidata.googleusercontent.com/caldav/v2";
        private const string CalendarBaseUrlFormat = BaseUrl + "/{0}";

        #region Implementation of ICalDAVEndPoint

        public string EventsUrlFormat { get { return string.Concat(CalendarBaseUrlFormat, "/events"); } }
        public string EventUrlFormat { get { return string.Concat(EventsUrlFormat, "/{1}"); } }

        #endregion
    }
}