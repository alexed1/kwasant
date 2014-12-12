using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.DataTypes;
using KwasantICS.DDay.iCal.Structs;
using Segment;
using Utilities;
using StructureMap;
using IEvent = KwasantCore.Interfaces.IEvent;
using AutoMapper;
using ParticipationStatus = KwasantICS.DDay.iCal.ParticipationStatus;

namespace KwasantCore.Services
{
    public class Event : Interfaces.IEvent
    {
        private readonly IMappingEngine _mappingEngine;
        private readonly Invitation _invitation;
        private readonly IBookingRequest _bookingRequest;
        private readonly Attendee _attendee;

        public Event(IMappingEngine mappingEngine, Invitation invitation, Attendee attendee)
        {
            if (mappingEngine == null)
                throw new ArgumentNullException("mappingEngine");
            if (invitation == null)
                throw new ArgumentNullException("invitation");
            if (attendee == null)
                throw new ArgumentNullException("attendee");
            _mappingEngine = mappingEngine;
            _invitation = invitation;
            _attendee = attendee;
            _bookingRequest = ObjectFactory.GetInstance<IBookingRequest>();
        }

        //this is called when a booker clicks on the calendar to create a new event. The form has not yet been filled out, so only 
        //some info about the event is known.
        public void Create(EventDO curEventDO, IUnitOfWork uow)
        {
            curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) && curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate);

            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.BookingRequest = bookingRequestDO;            
            curEventDO.CreatedBy = bookingRequestDO.Customer;
            curEventDO.CreatedByID = bookingRequestDO.Customer.Id;
            
            bookingRequestDO.Events.Add(curEventDO);

            var curCalendar = bookingRequestDO.Customer.Calendars.FirstOrDefault();
            if (curCalendar == null)
                throw new EntityNotFoundException<CalendarDO>("No calendars found for this user.");
			
            _bookingRequest.ExtractEmailAddresses(uow, curEventDO);

            curEventDO.EventStatus = EventState.Booking;
        }

        public EventDO Create(IUnitOfWork uow, int bookingRequestID, string startDate, string endDate)
        {
            var curEventDO = new EventDO();
            curEventDO.StartDate = DateTime.Parse(startDate);
            curEventDO.EndDate = DateTime.Parse(endDate);
            curEventDO.BookingRequestID = bookingRequestID;
            Create(curEventDO, uow);
            return curEventDO;
        }

        public void Delete(int eventID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(e => e.Id == eventID);
                if (eventDO != null)
                {
                    var oldStatus = eventDO.EventStatus;
                    eventDO.EventStatus = EventState.Deleted;
                    var hasSentEmails = eventDO.Emails.Any(e => e.EmailStatus == EmailState.Sent);
                    if (oldStatus != EventState.Draft && oldStatus != EventState.Deleted && hasSentEmails)
                    {
                        GenerateInvitations(uow, eventDO);
                    }
                    uow.SaveChanges();
                }
            }
        }

        public List<InvitationDO> GenerateInvitations(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> newAttendees = null, String extraBodyMessage = null)
        {
            if (newAttendees == null)
                newAttendees = new List<AttendeeDO>();
            var existingAttendees = eventDO.Attendees.Where(a => !newAttendees.Select(na => na.EmailAddress.Address).Contains(a.EmailAddress.Address));

            var invitations = new List<InvitationDO>();

            if (eventDO.EventStatus == EventState.Deleted)
            {
                invitations.AddRange(existingAttendees
                    .Select(newAttendee => _invitation.Generate(uow, InvitationType.CancelNotification, newAttendee, eventDO, extraBodyMessage))
                    .Where(i => i != null));
            }
            else
            {
                invitations.AddRange(newAttendees.Select(newAttendee => _invitation.Generate(uow, InvitationType.InitialInvite, newAttendee, eventDO, extraBodyMessage)).Where(i => i != null));
                invitations.AddRange(existingAttendees.Select(existingAttendee => _invitation.Generate(uow, InvitationType.ChangeNotification, existingAttendee, eventDO, extraBodyMessage)).Where(i => i != null));
            }
            return invitations;
        }

        public EventDO ProcessChanges(IUnitOfWork uow, EventDO updatedEventInfo, bool mergeEvents, out List<AttendeeDO> newAttendees)
        {
            if (updatedEventInfo.Id == 0)
                throw new ApplicationException(
                    "event should have been created and saved in #new, so Id should not be zero");
            var curEventDO = uow.EventRepository.GetByKey(updatedEventInfo.Id);
            if (curEventDO == null)
                throw new EntityNotFoundException<EventDO>();

            if (updatedEventInfo.Summary == null)
                updatedEventInfo.Summary = String.Empty;

            newAttendees = Update(uow, curEventDO, updatedEventInfo);

            curEventDO.EventStatus = updatedEventInfo.Summary.Contains("DRAFT")
                                         ? EventState.Draft
                                         : EventState.ReadyForDispatch;

            if (mergeEvents)
                MergeTimeSlots(uow, curEventDO);

            uow.SaveChanges();

            foreach (var attendeeDO in updatedEventInfo.Attendees)
            {
                var user = new User();
                var userDO = uow.UserRepository.GetOrCreateUser(attendeeDO.EmailAddress);
                if (user.GetMode(userDO) == CommunicationMode.Delegate)
                {
                    ObjectFactory.GetInstance<ITracker>()
                        .Track(userDO, "Customer", "InvitedAsPreCustomerAttendee",
                               new Dictionary<string, object>
                                   {
                                       {"BookingRequestId", curEventDO.BookingRequestID},
                                       {"EventID", curEventDO.Id}
                                   });
                }
            }

            uow.SaveChanges();

            AlertManager.EventBookedOrChanged(curEventDO.Id, curEventDO.BookingRequest.CustomerID);

            return curEventDO;
        }


        public void MergeTimeSlots(IUnitOfWork uow, EventDO updatedEvent)
        {
            //We want to merge existing events so we get continous blocks whenever they're 'filled in'.

            //We want to merge in the following situtations:

            //We have an existing event window ending at the same time as our new window starts.
            //This looks like this:
            //[Old--Event][New--Event]
            //With a merge result of this:
            //[New--------------Event]

            //We have an existing event window start at the same time as our new window ends.
            //This looks like this:
            //[New--Event][Old--Event]
            //With a merge result of this:
            //[New--------------Event]

            //We have an existing event window that partially overlaps our event from the beginning
            //This looks like this:
            //[Old--Event]
            //   [New--Event]
            //With a merge result of this:
            //[New-----Event]

            //We have an existing event window that partially overlaps our event from the end
            //This looks like this:
            //      [Old--Event]
            //   [New--Event]
            //With a merge result of this:
            //   [New-----Event]

            //We have an existing event that's fully contained within our event window
            //This looks like this:
            //    [Old--Event]
            //[New-----------Event]
            //With a merge result of this:
            //[New-----------Event]

            //All overlapping event windows are deleted from the system, with our new/modified event being expanded to encompass the overlaps.


            var overlaps =
                uow.EventRepository.GetQuery()
                    .Where(ew =>
                        //Don't include deleted events
                        ew.EventStatus != EventState.Deleted &&

                        ew.Id != updatedEvent.Id && ew.CalendarID == updatedEvent.CalendarID &&
                            //If the existing event starts at or before our start date, and ends at or after our start date
                        ((ew.StartDate <= updatedEvent.StartDate && ew.EndDate >= updatedEvent.StartDate) ||
                            //If the existing event starts at or after our end date, and ends at or before our end date
                         (ew.StartDate <= updatedEvent.EndDate && ew.EndDate >= updatedEvent.EndDate) ||
                            //If the existing event is entirely within our new date
                         (ew.StartDate >= updatedEvent.StartDate && ew.EndDate <= updatedEvent.EndDate)
                         )).ToList();

            //We want to get the min/max start dates _including_ our new event window
            var fullSet = overlaps.Union(new[] { updatedEvent }).ToList();

            var newStart = fullSet.Min(ew => ew.StartDate);
            var newEnd = fullSet.Max(ew => ew.EndDate);

            //Delete all overlaps
            foreach (var overlap in overlaps)
                uow.EventRepository.Remove(overlap);

            //Potentially expand our new/modified event window
            updatedEvent.StartDate = newStart;
            updatedEvent.EndDate = newEnd;
        }

        public List<AttendeeDO> Update(IUnitOfWork uow, EventDO eventDO, EventDO updatedEventInfo)
        {
            _mappingEngine.Map(updatedEventInfo, eventDO);
            return UpdateAttendees(uow, eventDO, updatedEventInfo.Attendees);
        }

        public List<AttendeeDO> UpdateAttendees(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> updatedAttendeeList)
        {
            List<AttendeeDO> newAttendees = new List<AttendeeDO>();

            var attendeesToDelete = eventDO.Attendees.Where(attendee => !updatedAttendeeList.Select(a => a.EmailAddress.Address).Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
            {
                eventDO.Attendees.Remove(attendeeToDelete);
                uow.AttendeeRepository.Remove(attendeeToDelete);
            }

            foreach (var attendee in updatedAttendeeList.Where(att => !eventDO.Attendees.Select(a => a.EmailAddress.Address).Contains(att.EmailAddress.Address)))
            {
                eventDO.Attendees.Add(attendee);
                newAttendees.Add(attendee);
            }

            if (eventDO.EventStatus == EventState.Booking)
                newAttendees = eventDO.Attendees;

            return newAttendees;
        }

        public static iCalendar GenerateICSCalendarStructure(EventDO eventDO)
        {
            if (eventDO == null)
                throw new ArgumentNullException("eventDO");

            IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            var invitation = ObjectFactory.GetInstance<Invitation>();
            string fromEmail = configRepository.Get("EmailFromAddress_DelegateMode");
            string fromName = configRepository.Get("EmailFromName_DelegateMode");
            fromName = String.Format(fromName, invitation.GetOriginatorName(eventDO));

            iCalendar ddayCalendar = new iCalendar();
            
            DDayEvent dDayEvent = new DDayEvent();

            TimeZoneInfo timeZone;
            if (eventDO.BookingRequest != null)
            {
                timeZone = eventDO.BookingRequest.Customer.GetOrGuessTimeZone();
            }
            else
            {
                var possibleZones = TimeZoneInfo.GetSystemTimeZones().Where(tzi => tzi.GetUtcOffset(DateTime.Now) == eventDO.StartDate.Offset);
                timeZone = possibleZones.FirstOrDefault();
            }

            //configure start and end time
            dDayEvent.IsAllDay = eventDO.IsAllDay;

            IDateTime start;
            IDateTime end;
            if (timeZone != null) //If we find a potential timezone, use it
            {
                var calTimeZone = iCalTimeZone.FromSystemTimeZone(timeZone);
                ddayCalendar.AddTimeZone(calTimeZone);

                start = new iCalDateTime(eventDO.StartDate.DateTime)
                {
                    IsUniversalTime = false,
                    TZID = calTimeZone.TZID
                };

                end = new iCalDateTime(eventDO.EndDate.DateTime)
                {
                    IsUniversalTime = false,
                    TZID = calTimeZone.TZID
                };
            }
            else //If we don't find a matching timezone, use UTC.
            {
                start = new iCalDateTime(eventDO.StartDate.ToUniversalTime().DateTime) { IsUniversalTime = true };
                end = new iCalDateTime(eventDO.EndDate.ToUniversalTime().DateTime) { IsUniversalTime = true };
            }

            if (!eventDO.IsAllDay && !start.HasTime)
                start.HasTime = true;
            dDayEvent.DTStart = start;

            if (!eventDO.IsAllDay && !end.HasTime)
                end.HasTime = true;
            dDayEvent.DTEnd = end;
            
            dDayEvent.DTStamp = new iCalDateTime(DateTime.UtcNow);
            dDayEvent.LastModified = new iCalDateTime(DateTime.UtcNow);

            //configure text fields
            dDayEvent.Location = eventDO.Location;
            dDayEvent.Description = eventDO.Description;
            dDayEvent.Summary = eventDO.Summary;
            dDayEvent.UID = eventDO.ExternalGUID;

            //more attendee configuration
            foreach (AttendeeDO attendee in eventDO.Attendees)
            {
                dDayEvent.Attendees.Add(new KwasantICS.DDay.iCal.DataTypes.Attendee()
                {
                    CommonName = attendee.Name,
                    Type = "INDIVIDUAL",
                    Role = "REQ-PARTICIPANT",
                    ParticipationStatus = ParticipationStatus.NeedsAction,
                    RSVP = true,
                    Value = new Uri("mailto:" + attendee.EmailAddress.Address),
                });
                attendee.Event = eventDO;
            }

            //final assembly of event
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };
            if (eventDO.EventStatus == EventState.Deleted)
            {
                dDayEvent.Status = EventStatus.Cancelled;
            }
            else
            {
                dDayEvent.Status = EventStatus.Confirmed;
            }
            ddayCalendar.Events.Add(dDayEvent);
            if (eventDO.EventStatus == EventState.Deleted)
            {
                ddayCalendar.Method = CalendarMethods.Cancel;
            }
            else
            {
                ddayCalendar.Method = CalendarMethods.Request;
            }

            return ddayCalendar;
        }

        public static EventDO CreateEventFromICSCalendar(IUnitOfWork uow, iCalendar iCalendar)
        {
            if (iCalendar.Events.Count == 0)
                throw new ArgumentException("iCalendar has no events.");
            var icsEvent = iCalendar.Events[0];
            return new EventDO()
            {
                Category = icsEvent.Categories != null ? icsEvent.Categories.FirstOrDefault() : null,
                Class = icsEvent.Class,
                Description = icsEvent.Description,
                IsAllDay = icsEvent.IsAllDay,
                StartDate = icsEvent.Start.UTC,
                EndDate = icsEvent.End.UTC,
                Location = icsEvent.Location,
                Sequence = icsEvent.Sequence,
                Summary = icsEvent.Summary,
                Transparency = icsEvent.Transparency.ToString(),
                CreateDate = icsEvent.Created != null ? icsEvent.Created.UTC : default(DateTimeOffset),
                Attendees = icsEvent.Attendees
                    .Where(a => a.Value != null)
                    .Select(a => new AttendeeDO()
                    {
                        EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(a.Value.OriginalString.Remove(0, a.Value.Scheme.Length + 1)),
                        Name = a.CommonName
                    })
                    .ToList(),
            };
        }

    }
}