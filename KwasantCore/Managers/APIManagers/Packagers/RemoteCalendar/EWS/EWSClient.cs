using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantICS.Collections;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces;
using KwasantICS.DDay.iCal.Interfaces.Components;
using KwasantICS.DDay.iCal.Interfaces.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.General;
using KwasantICS.DDay.iCal.Structs;
using Microsoft.Exchange.WebServices.Data;
using Attendee = Microsoft.Exchange.WebServices.Data.Attendee;
using Task = System.Threading.Tasks.Task;

namespace KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar.EWS
{
    class EWSClient : IRemoteCalendarServiceClient
    {
        class EWSEvent : IEvent
        {
            private readonly Appointment _appointment;

            public EWSEvent(Appointment appointment)
            {
                _appointment = appointment;
                Name = _appointment.LastModifiedName;
                Attendees = _appointment
                    .RequiredAttendees.Union(_appointment.OptionalAttendees)
                    .Select<Attendee, IAttendee>(a => new KwasantICS.DDay.iCal.DataTypes.Attendee(a.Address))
                    .ToList();
                Categories = _appointment.Categories.ToList();
                Summary = _appointment.Subject;
                Organizer = new Organizer(_appointment.Organizer.Address) { CommonName = _appointment.Organizer.Name };
                Start = new iCalDateTime(_appointment.Start, _appointment.StartTimeZone.Id);
                End = new iCalDateTime(_appointment.End, _appointment.EndTimeZone.Id);
                IsAllDay = _appointment.IsAllDayEvent;
                Created = new iCalDateTime(_appointment.DateTimeCreated);
                Location = _appointment.Location;
                Description = _appointment.TextBody;
                if (_appointment.ICalDateTimeStamp.HasValue)
                {
                    DTStamp = new iCalDateTime(_appointment.ICalDateTimeStamp.Value);
                }
                UID = _appointment.ICalUid;
                Url = new Uri(_appointment.NetShowUrl);
            }

            public event EventHandler<ObjectEventArgs<string, string>> GroupChanged;
            public string Group { get; set; }
            public bool IsLoaded { get; private set; }
            public event EventHandler Loaded;
            public void OnLoaded()
            {
                throw new NotImplementedException();
            }

            public void CopyFrom(ICopyable obj)
            {
                throw new NotImplementedException();
            }

            public T Copy<T>()
            {
                throw new NotImplementedException();
            }

            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public object GetService(string name)
            {
                throw new NotImplementedException();
            }

            public T GetService<T>()
            {
                throw new NotImplementedException();
            }

            public T GetService<T>(string name)
            {
                throw new NotImplementedException();
            }

            public void SetService(string name, object obj)
            {
                throw new NotImplementedException();
            }

            public void SetService(object obj)
            {
                throw new NotImplementedException();
            }

            public void RemoveService(Type type)
            {
                throw new NotImplementedException();
            }

            public void RemoveService(string name)
            {
                throw new NotImplementedException();
            }

            public string Name { get; set; }
            public ICalendarObject Parent { get; set; }
            public ICalendarObjectList<ICalendarObject> Children { get; private set; }
            public IICalendar Calendar { get; private set; }
            public IICalendar iCalendar { get; private set; }
            public int Line { get; set; }
            public int Column { get; set; }
            public ICalendarPropertyList Properties { get; private set; }
            public event EventHandler<ObjectEventArgs<string, string>> UIDChanged;
            public string UID { get; set; }
            public IList<IAttendee> Attendees { get; set; }
            public IList<string> Comments { get; set; }
            public IDateTime DTStamp { get; set; }
            public IOrganizer Organizer { get; set; }
            public IList<IRequestStatus> RequestStatuses { get; set; }
            public Uri Url { get; set; }
            public void ClearEvaluation()
            {
                throw new NotImplementedException();
            }

            public IList<Occurrence> GetOccurrences(IDateTime dt)
            {
                throw new NotImplementedException();
            }

            public IList<Occurrence> GetOccurrences(DateTime dt)
            {
                throw new NotImplementedException();
            }

            public IList<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
            {
                throw new NotImplementedException();
            }

            public IList<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
            {
                throw new NotImplementedException();
            }

            public IDateTime DTStart { get; set; }
            public IDateTime Start { get; set; }
            public IList<IPeriodList> ExceptionDates { get; set; }
            public IList<IRecurrencePattern> ExceptionRules { get; set; }
            public IList<IPeriodList> RecurrenceDates { get; set; }
            public IList<IRecurrencePattern> RecurrenceRules { get; set; }
            public IDateTime RecurrenceID { get; set; }
            public ICalendarObjectList<IAlarm> Alarms { get; private set; }
            public IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime)
            {
                throw new NotImplementedException();
            }

            public IList<IAttachment> Attachments { get; set; }
            public IList<string> Categories { get; set; }
            public string Class { get; set; }
            public IList<string> Contacts { get; set; }
            public IDateTime Created { get; set; }
            public string Description { get; set; }
            public IDateTime LastModified { get; set; }
            public int Priority { get; set; }
            public IList<string> RelatedComponents { get; set; }
            public int Sequence { get; set; }
            public string Summary { get; set; }
            public IDateTime DTEnd { get; set; }
            public TimeSpan Duration { get; set; }
            public IDateTime End { get; set; }
            public bool IsAllDay { get; set; }
            public IGeographicLocation GeographicLocation { get; set; }
            public string Location { get; set; }
            public IList<string> Resources { get; set; }
            public EventStatus Status { get; set; }
            public string WorkflowState { get; set; }
            public TransparencyType Transparency { get; set; }
            public bool IsActive()
            {
                throw new NotImplementedException();
            }
        }

        private readonly IRemoteCalendarAuthDataDO _authData;
        private readonly Lazy<ExchangeService> _lazyExchangeService;

        public EWSClient(IRemoteCalendarAuthDataDO authData)
        {
            _authData = authData;
            _lazyExchangeService = new Lazy<ExchangeService>(CreateServiceClient);
        }

        private ExchangeService CreateServiceClient()
        {
            var serviceClient = new ExchangeService();
            var jsonStore = new JSONDataStore(() => _authData.AuthData, s => _authData.AuthData = s);
            var email = jsonStore.GetAsync<string>("email").Result;
            var password = jsonStore.GetAsync<string>("password").Result;
            serviceClient.Credentials = new WebCredentials(email, password);
            serviceClient.AutodiscoverUrl(email, ValidateRedirectionUrlCallback);
            return serviceClient;
        }

        private bool ValidateRedirectionUrlCallback(string redirectionUrl)
        {
            return true;
        }

        private ExchangeService ExchangeService { get { return _lazyExchangeService.Value; } }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(IRemoteCalendarLinkDO calendarLink, DateTimeOffset @from, DateTimeOffset to)
        {
            var folderId = new FolderId(calendarLink.RemoteCalendarHref);
            var appointments = ExchangeService.FindAppointments(
                folderId,
                new CalendarView(
                    TimeZoneInfo.ConvertTime(from, ExchangeService.TimeZone).DateTime,
                    TimeZoneInfo.ConvertTime(to, ExchangeService.TimeZone).DateTime));
            return appointments.Select(a => new EWSEvent(a)).ToArray();
        }

        public async Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, IEvent calendarEvent)
        {
            throw new NotImplementedException();
        }

        public async Task<IDictionary<string, string>> GetCalendarsAsync(IRemoteCalendarAuthDataDO authData)
        {
            var calendarFolder = Folder.Bind(ExchangeService, WellKnownFolderName.Calendar);
            return new Dictionary<string, string>()
                {
                    {
                        calendarFolder.Id.UniqueId,
                        calendarFolder.DisplayName
                    }
                };
        }
    }
}
