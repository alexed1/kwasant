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
        class EventToAppointmentAdapter
        {
            public IEvent AdaptAppointment(Appointment appointment)
            {
                var e = new DDayEvent();
                e.Attendees = appointment
                    .RequiredAttendees.Union(appointment.OptionalAttendees)
                    .Select<Attendee, IAttendee>(a => new KwasantICS.DDay.iCal.DataTypes.Attendee(a.Address))
                    .ToList();
                e.Categories = appointment.Categories.ToList();
                e.Summary = appointment.Subject;
                e.Organizer = new Organizer(appointment.Organizer.Address) { CommonName = appointment.Organizer.Name };
                e.Start = new iCalDateTime(appointment.Start, appointment.StartTimeZone.Id);
                e.End = new iCalDateTime(appointment.End, appointment.EndTimeZone.Id);
                e.IsAllDay = appointment.IsAllDayEvent;
                e.Created = new iCalDateTime(appointment.DateTimeCreated);
                e.Location = appointment.Location;
                e.Description = appointment.TextBody;
                if (appointment.ICalDateTimeStamp.HasValue)
                {
                    e.DTStamp = new iCalDateTime(appointment.ICalDateTimeStamp.Value);
                }
                e.UID = appointment.ICalUid;
                e.Url = new Uri(appointment.NetShowUrl);
                return e;
            }

            public Appointment AdaptEvent(IEvent e, ExchangeService exchangeService)
            {
                var appointment = new Appointment(exchangeService);
                foreach (var attendee in e.Attendees)
                {
                    var a = new Attendee(attendee.CommonName, attendee.Value.Authority);
                    switch (attendee.Role)
                    {
                        case "REQ-PARTICIPANT":
                            appointment.RequiredAttendees.Add(a);
                            break;
                        case "OPT-PARTICIPANT":
                            appointment.OptionalAttendees.Add(a);
                            break;
                    }
                }
                appointment.Location = e.Location;
                appointment.Categories = new StringList(e.Categories);
                appointment.Subject = e.Summary;
                appointment.Organizer.Name = e.Organizer.CommonName;
                appointment.Organizer.Address = e.Organizer.Value.Authority;
                appointment.Start = e.Start.Date;
                appointment.StartTimeZone = TimeZoneInfo.FromSerializedString(e.Start.TimeZoneName);
                appointment.End = e.End.Date;
                appointment.EndTimeZone = TimeZoneInfo.FromSerializedString(e.End.TimeZoneName);
                appointment.IsAllDayEvent = e.IsAllDay;
                appointment.Body = new TextBody(e.Description);
                appointment.ICalUid = e.UID;
                return appointment;
            }
        }

        private readonly IRemoteCalendarAuthDataDO _authData;
        private readonly Lazy<ExchangeService> _lazyExchangeService;
        private readonly EventToAppointmentAdapter _adapter;

        public EWSClient(IRemoteCalendarAuthDataDO authData)
        {
            _authData = authData;
            _lazyExchangeService = new Lazy<ExchangeService>(CreateServiceClient);
            _adapter = new EventToAppointmentAdapter();
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
            return appointments.Select(a => _adapter.AdaptAppointment(a)).ToArray();
        }

        public async Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, IEvent calendarEvent)
        {
            var a = _adapter.AdaptEvent(calendarEvent, ExchangeService);
            var folderId = new FolderId(calendarLink.RemoteCalendarHref);
            a.Save(folderId);
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
