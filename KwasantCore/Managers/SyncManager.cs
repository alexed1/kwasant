﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.CalDAV;
using KwasantCore.Services;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Interfaces;

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

        public async Task SyncNowAsync(IUnitOfWork uow, ICalendar calendar, DateTimeOffset lastSynchronization)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (calendar == null)
                throw new ArgumentNullException("calendar");
            var client = _clientFactory.Create(calendar.Owner);
            if (client == null)
                return;
            DateTimeOffset from, to;
            GetPeriod(out from, out to);
            var remoteEvents = await client.GetEventsAsync(calendar, from, to);
            var incomingEvents = remoteEvents.Select(Event.CreateEventFromICSCalendar).ToArray();
            // search for events that overlap the period as CalDAV does. Not sure about equality sign.
            var existingEvents = uow.EventRepository.GetQuery().Where(e => e.StartDate <= to && e.EndDate >= from).ToList();

            foreach (var incomingEvent in incomingEvents)
            {
                var existingEvent = existingEvents.FirstOrDefault(e => _eventComparer.Equals(incomingEvent, e));
                if (existingEvent != null)
                {
                    existingEvent.Attendees.Clear();
                    foreach (var incomingAttendee in incomingEvent.Attendees)
                    {
                        var attendee =
                            uow.AttendeeRepository.GetQuery()
                            .FirstOrDefault(a => a.EmailAddress.Address == incomingAttendee.EmailAddress.Address && a.EventID == existingEvent.Id);
                        if (attendee == null)
                        {
                            var existingEmailAddress =
                                uow.EmailAddressRepository.GetOrCreateEmailAddress(incomingAttendee.EmailAddress.Address);
                            attendee = incomingAttendee;
                            attendee.EmailAddress = existingEmailAddress;
                            attendee.EmailAddressID = existingEmailAddress.Id;
                            attendee.Event = existingEvent;
                            attendee.EventID = existingEvent.Id;
                        }
                        existingEvent.Attendees.Add(attendee);
                    }
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
                    incomingEvent.Calendar = (CalendarDO)calendar;
                    incomingEvent.CalendarID = calendar.Id;
                    incomingEvent.CreatedBy = calendar.Owner;
                    incomingEvent.CreatedByID = calendar.Owner.Id;
                    if (!incomingEvent.Attendees.Any(a => string.Equals(a.EmailAddress.Address, calendar.Owner.EmailAddress.Address)))
                    {
                        incomingEvent.Attendees.Add(new AttendeeDO()
                                                        {
                                                            EmailAddress = calendar.Owner.EmailAddress,
                                                            EmailAddressID = calendar.Owner.EmailAddressID,
                                                            Event = incomingEvent,
                                                            Name = calendar.Owner.UserName
                                                        });
                    }
                    uow.EventRepository.Add(incomingEvent);
                }
            }

            var createdByKwasant = existingEvents.Where(e => e.DateCreated >= lastSynchronization).ToList();
            foreach (var created in createdByKwasant)
            {
                var iCalendarEvent = Event.GenerateICSCalendarStructure(created);
                await client.CreateEventAsync(calendar, iCalendarEvent);
            }

            var deletedByRemote = existingEvents.Where(e => e.DateCreated < lastSynchronization).ToList();
            foreach (var deleted in deletedByRemote)
            {
                uow.EventRepository.Remove(deleted);
            }
        }
    }
}
