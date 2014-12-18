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
            private string AdaptAppointmentAddress(string address)
            {
                return string.Concat("mailto:", address);
            }

            private IAttendee AdaptAppointmentAttendee(Attendee a, string role)
            {
                return new KwasantICS.DDay.iCal.DataTypes.Attendee(AdaptAppointmentAddress(a.Address))
                    {
                        CommonName = a.Name,
                        Role = role
                    };
            }

            public IEvent AdaptAppointment(Appointment appointment)
            {
                var e = new DDayEvent();
                e.Attendees = appointment.RequiredAttendees
                    .Select(a => AdaptAppointmentAttendee(a, "REQ-PARTICIPANT"))
                    .Union(
                        appointment.OptionalAttendees
                            .Select(a => AdaptAppointmentAttendee(a, "OPT-PARTICIPANT")))
                    .ToList();
                e.Categories = appointment.Categories.ToList();
                e.Summary = appointment.Subject;
                e.Organizer = new Organizer(AdaptAppointmentAddress(appointment.Organizer.Address))
                    {
                        CommonName = appointment.Organizer.Name
                    };
                e.Start = new iCalDateTime(appointment.Start, TimeZoneInfo.Utc.Id);
                e.End = new iCalDateTime(appointment.End, TimeZoneInfo.Utc.Id);
                e.IsAllDay = appointment.IsAllDayEvent;
                e.Created = new iCalDateTime(appointment.DateTimeCreated, TimeZoneInfo.Utc.Id);
                e.Location = appointment.Location;
                e.Description = appointment.TextBody;
                e.Sequence = appointment.AppointmentSequenceNumber;
                if (appointment.ICalDateTimeStamp.HasValue)
                {
                    e.DTStamp = new iCalDateTime(appointment.ICalDateTimeStamp.Value);
                }
                e.UID = appointment.ICalUid;
                return e;
            }

            private string AdaptEventAddress(Uri address)
            {
                return address.AbsoluteUri.Remove(0, address.GetLeftPart(UriPartial.Scheme).Length);
            }

            public Appointment AdaptEvent(IEvent e, ExchangeService exchangeService)
            {
                var appointment = new Appointment(exchangeService);
                foreach (var attendee in e.Attendees)
                {
                    var a = new Attendee(attendee.CommonName, AdaptEventAddress(attendee.Value));
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
                appointment.Subject = e.Summary;
                appointment.Location = e.Location;
                appointment.Body = new TextBody(e.Description);
                appointment.Categories = new StringList(e.Categories);
                // EWS doesn't allow to set Organizer from code, it will be set to remote calendar account automatically
                // appointment.Organizer = ...
                appointment.Start = e.Start.UTC;
                appointment.End = e.End.UTC;
                appointment.IsAllDayEvent = e.IsAllDay;
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
            var folder = CalendarFolder.Bind(ExchangeService, folderId, new PropertySet());
            var view = new CalendarView(
                TimeZoneInfo.ConvertTime(from, ExchangeService.TimeZone).DateTime,
                TimeZoneInfo.ConvertTime(to, ExchangeService.TimeZone).DateTime);
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
            var appointments = folder.FindAppointments(view);
            var propSet = new PropertySet(AppointmentSchema.Subject,
                AppointmentSchema.Organizer,
                AppointmentSchema.RequiredAttendees, AppointmentSchema.OptionalAttendees,
                AppointmentSchema.Start, //AppointmentSchema.StartTimeZone,
                AppointmentSchema.End, //AppointmentSchema.EndTimeZone,
                AppointmentSchema.TimeZone, AppointmentSchema.IsAllDayEvent,
                AppointmentSchema.Categories, AppointmentSchema.Location, AppointmentSchema.TextBody,
                AppointmentSchema.AppointmentType, AppointmentSchema.AppointmentState, AppointmentSchema.AppointmentSequenceNumber,
                AppointmentSchema.DateTimeCreated, 
                AppointmentSchema.ICalDateTimeStamp, AppointmentSchema.ICalUid);
            ExchangeService.LoadPropertiesForItems(appointments, propSet);
            return appointments.Select(a => _adapter.AdaptAppointment(a)).ToArray();
        }

        public async Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, IEvent calendarEvent)
        {
            var a = _adapter.AdaptEvent(calendarEvent, ExchangeService);
            var folderId = new FolderId(calendarLink.RemoteCalendarHref);
            a.Save(folderId, SendInvitationsMode.SendToNone);
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
