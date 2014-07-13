using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.CalDAV;
using KwasantCore.Services;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Interfaces;
using Utilities.Logging;

namespace KwasantCore.Managers
{
    public class SyncManager
    {
        class EventComparer : IEqualityComparer<EventDO>
        {
            #region Implementation of IEqualityComparer<in EventDO>

            public bool Equals(EventDO x, EventDO y)
            {
                return x.StartDate == y.StartDate
                    && x.EndDate == y.EndDate
                    && string.Equals(x.Summary, y.Summary);
            }

            public int GetHashCode(EventDO obj)
            {
                unchecked
                {
                    var hashCode = obj.StartDate.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.EndDate.GetHashCode();
                    hashCode = (hashCode * 397) ^ (obj.Summary != null ? obj.Summary.GetHashCode() : 0);
                    return hashCode;
                }
            }

            #endregion
        }

        private readonly ICalDAVClientFactory _clientFactory;
        private readonly EventComparer _eventComparer = new EventComparer();

        public SyncManager(ICalDAVClientFactory clientFactory)
        {
            if (clientFactory == null)
                throw new ArgumentNullException("clientFactory");
            _clientFactory = clientFactory;
        }

        public void GetPeriod(out DateTimeOffset from, out DateTimeOffset to)
        {
            from = DateTimeOffset.UtcNow;
            to = from + TimeSpan.FromDays(365);
        }

        public async Task SyncNowAsync(IUnitOfWork uow, ICalendar calendar)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (calendar == null)
                throw new ArgumentNullException("calendar");
            if (calendar.Owner == null)
                throw new ArgumentException("Calendar owner is null", "calendar");

            // in the future there might be some tables in the database with data for calendar providers and authorization.
            // + provider's endpoint data
            // + user's oauth tokens
            // + lastSynchronized for each provider
            var providers = new List<string>();
            if (calendar.Owner.GrantedAccessToGoogleCalendar)
            {
                providers.Add("Google");
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            DateTimeOffset from, to;
            GetPeriod(out from, out to);

            foreach (var provider in providers)
            {
                var client = _clientFactory.Create(calendar.Owner, provider);
                try
                {
                    await SyncNowWithClientAsync(uow, calendar, client, @from, to);
                }
                catch (Exception)
                {
                    Logger.GetLogger().ErrorFormat("Error occurred on calendar synchronization with '{0}' for calendar: {1}.", provider, calendar.Id);
                }
            }

            calendar.LastSynchronized = now;

        }

        private async Task SyncNowWithClientAsync(IUnitOfWork uow, ICalendar calendar, ICalDAVClient client, DateTimeOffset @from,
                                            DateTimeOffset to)
        {
            Func<EventDO, bool> eventPredictor = e => e.StartDate <= to && e.EndDate >= @from;
            var remoteEvents = await client.GetEventsAsync(calendar, @from, to);
            var incomingEvents = remoteEvents.Select(Event.CreateEventFromICSCalendar).Where(eventPredictor).ToArray();
            var existingEvents = calendar.Events.Where(eventPredictor).ToList();

            foreach (var incomingEvent in incomingEvents)
            {
                if (
                    !incomingEvent.Attendees.Any(a => string.Equals(a.EmailAddress.Address, calendar.Owner.EmailAddress.Address)))
                {
                    incomingEvent.Attendees.Add(new AttendeeDO()
                                                    {
                                                        EmailAddress = calendar.Owner.EmailAddress,
                                                        EmailAddressID = calendar.Owner.EmailAddressID,
                                                        Event = incomingEvent,
                                                        Name = calendar.Owner.UserName
                                                    });
                }

                var existingEvent = existingEvents.FirstOrDefault(e => _eventComparer.Equals(incomingEvent, e));
                if (existingEvent != null)
                {
                    var provedAttendees = new List<AttendeeDO>(existingEvent.Attendees.Count);
                    foreach (var incomingAttendee in incomingEvent.Attendees)
                    {
                        var attendee =
                            existingEvent.Attendees.FirstOrDefault(
                                a => a.EmailAddress.Address == incomingAttendee.EmailAddress.Address);
                        if (attendee == null)
                        {
                            var existingEmailAddress =
                                uow.EmailAddressRepository.GetOrCreateEmailAddress(incomingAttendee.EmailAddress.Address);
                            attendee = incomingAttendee;
                            attendee.EmailAddress = existingEmailAddress;
                            attendee.EmailAddressID = existingEmailAddress.Id;
                            attendee.Event = existingEvent;
                            attendee.EventID = existingEvent.Id;
                            existingEvent.Attendees.Add(attendee);
                        }
                        provedAttendees.Add(attendee);
                    }
                    existingEvent.Attendees.RemoveAll(a => !provedAttendees.Contains(a));

                    existingEvent.Category = incomingEvent.Category;
                    existingEvent.Class = incomingEvent.Class;
                    existingEvent.Description = incomingEvent.Description;
                    existingEvent.Location = incomingEvent.Location;
                    existingEvent.Sequence = incomingEvent.Sequence;

                    existingEvents.Remove(existingEvent);
                }
                else
                {
                    // created by remote
                    incomingEvent.StateID = EventState.DispatchCompleted;
                    incomingEvent.CreateTypeID = EventCreateType.GoogleCalendar;
                    incomingEvent.Calendar = (CalendarDO) calendar;
                    incomingEvent.CalendarID = calendar.Id;
                    incomingEvent.CreatedBy = calendar.Owner;
                    incomingEvent.CreatedByID = calendar.Owner.Id;
                    calendar.Events.Add(incomingEvent);
                }
            }

            var createdByKwasant = existingEvents.Where(e => e.DateCreated >= calendar.LastSynchronized).ToList();
            foreach (var created in createdByKwasant)
            {
                var iCalendarEvent = Event.GenerateICSCalendarStructure(created);
                await client.CreateEventAsync(calendar, iCalendarEvent);
            }

            var deletedByRemote = existingEvents.Where(e => e.DateCreated < calendar.LastSynchronized).ToList();
            foreach (var deleted in deletedByRemote)
            {
                calendar.Events.Remove(deleted);
            }
        }
    }
}
