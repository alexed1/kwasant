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

        public async Task SyncNowAsync(IUnitOfWork uow, IUser user)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (user == null)
                throw new ArgumentNullException("user");

            DateTimeOffset from, to;
            GetPeriod(out from, out to);

            foreach (var authData in user.RemoteCalendarAuthData)
            {
                try
                {
                    await SyncProviderAsync(uow, authData, @from, to);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error(string.Format("Error occurred on calendar synchronization with '{0}'.", authData.Provider.Name), ex);
                }
            }
        }

        private async Task SyncProviderAsync(IUnitOfWork uow, IRemoteCalendarAuthData authData, DateTimeOffset @from,
                                            DateTimeOffset to)
        {
            var client = _clientFactory.Create(authData);
            // TODO: obtain a real list from remote calendar provider
            var remoteCalendars = new[] {authData.User.EmailAddress.Address};
            foreach (var remoteName in remoteCalendars)
            {
                var calendarLink = uow.RemoteCalendarLinkRepository.GetOrCreate(authData, remoteName);
                try
                {
                    calendarLink.DateSynchronizationAttempted = DateTimeOffset.UtcNow;
                    await SyncCalendarAsync(uow, @from, to, client, calendarLink);

                    calendarLink.LastSynchronizationResult = "Success";
                    calendarLink.DateSynchronized = calendarLink.DateSynchronizationAttempted;
                }
                catch (Exception ex)
                {
                    calendarLink.LastSynchronizationResult = string.Concat("Error: ", ex.Message);
                    Logger.GetLogger().Error(
                        string.Format("Error occurred on calendar '{0}' synchronization with '{1} @ {2}'.",
                                      calendarLink.LocalCalendar.Name,
                                      calendarLink.RemoteCalendarName,
                                      calendarLink.Provider.Name),
                        ex);
                }
            }
        }

        private async Task SyncCalendarAsync(IUnitOfWork uow, DateTimeOffset @from, DateTimeOffset to, ICalDAVClient client, IRemoteCalendarLink calendarLink)
        {
            Func<EventDO, bool> eventPredictor = e => e.StartDate <= to && e.EndDate >= @from;
            var remoteEvents = await client.GetEventsAsync(calendarLink, @from, to);
            var incomingEvents = remoteEvents.Select(Event.CreateEventFromICSCalendar).Where(eventPredictor).ToArray();
            var calendar = calendarLink.LocalCalendar;
            Func<EventDO, bool> existingEventPredictor =
                e => eventPredictor(e) && e.SyncStatusID == EventSyncStatus.SyncWithExternal;
            var existingEvents = calendar.Events.Where(existingEventPredictor).ToList();

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
                    incomingEvent.CreateTypeID = EventCreateType.RemoteCalendar;
                    incomingEvent.SyncStatusID = EventSyncStatus.SyncWithExternal;
                    incomingEvent.Calendar = (CalendarDO) calendar;
                    incomingEvent.CalendarID = calendar.Id;
                    incomingEvent.CreatedBy = calendar.Owner;
                    incomingEvent.CreatedByID = calendar.Owner.Id;
                    calendar.Events.Add(incomingEvent);
                }
            }

            var createdByKwasant = existingEvents.Where(e => e.DateCreated >= calendarLink.DateSynchronized).ToList();
            foreach (var created in createdByKwasant)
            {
                var iCalendarEvent = Event.GenerateICSCalendarStructure(created);
                await client.CreateEventAsync(calendarLink, iCalendarEvent);
            }

            var deletedByRemote = existingEvents.Where(e => e.DateCreated < calendarLink.DateSynchronized).ToList();
            foreach (var deleted in deletedByRemote)
            {
                calendar.Events.Remove(deleted);
            }
        }
    }
}
