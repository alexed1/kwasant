using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using Utilities;
using StructureMap;
using IEvent = Data.Interfaces.IEvent;
using AutoMapper;

namespace KwasantCore.Services
{
    public class Event : IEvent
    {
        private readonly IMappingEngine _mappingEngine;
        private readonly Invitation _invitation;

        public Event(IMappingEngine mappingEngine, Invitation invitation)
        {
            if (mappingEngine == null)
                throw new ArgumentNullException("mappingEngine");
            if (invitation == null)
                throw new ArgumentNullException("invitation");
            _mappingEngine = mappingEngine;
            _invitation = invitation;
        }

        //this is called when a booker clicks on the calendar to create a new event. The form has not yet been filled out, so only 
        //some info about the event is known.
        public void Create(EventDO curEventDO, IUnitOfWork uow)
        {
            curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) &&
                                  curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate);

            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.BookingRequest = bookingRequestDO;            
            curEventDO.CreatedBy = bookingRequestDO.User;
            curEventDO.CreatedByID = bookingRequestDO.User.Id;
            curEventDO.DateCreated = DateTimeOffset.UtcNow.ToOffset(bookingRequestDO.DateCreated.Offset);
            
            var curCalendar = bookingRequestDO.User.Calendars.FirstOrDefault();
            if (curCalendar == null)
                throw new EntityNotFoundException<CalendarDO>("No calendars found for this user.");
			
			var attendee = new Attendee();
            attendee.DetectEmailsFromBookingRequest(uow, curEventDO);

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

        public void InviteAttendees(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> newAttendees, List<AttendeeDO> existingAttendees)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (eventDO == null)
                throw new ArgumentNullException("eventDO");
            if (existingAttendees == null)
                throw new ArgumentNullException("existingAttendees");

            var invitations = GenerateInvitations(uow, eventDO, newAttendees, existingAttendees);
            foreach (var invitationDO in invitations)
            {
                _invitation.Dispatch(uow, invitationDO);
            }
            eventDO.EventStatus = EventState.DispatchCompleted;
        }

        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes
        public void InviteAttendees(IUnitOfWork uow, EventDO eventDO, EventDO updatedEventInfo, List<AttendeeDO> updatedAttendees)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (eventDO == null)
                throw new ArgumentNullException("eventDO");
            if (updatedEventInfo == null)
                throw new ArgumentNullException("updatedEventInfo");
            if (updatedAttendees == null)
                throw new ArgumentNullException("updatedAttendees");

            List<AttendeeDO> newAttendees; 
            List<AttendeeDO> existingAttendees;
            eventDO = Update(uow, eventDO, updatedEventInfo, updatedAttendees, out newAttendees, out existingAttendees);
            if (eventDO != null)
            {
                InviteAttendees(uow, eventDO, newAttendees, existingAttendees);
            }
        }
     
        public List<InvitationDO> GenerateInvitations(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> newAttendees, List<AttendeeDO> existingAttendees)
        {
            //This line is so that the Server object is compiled. Without this, Razor fails; since it's executed at runtime and the object has been optimized out when running tests.
            //var createdDate = eventDO.BookingRequest.DateCreated;
            //eventDO.StartDate = eventDO.StartDate.ToOffset(createdDate.Offset);
            //eventDO.EndDate = eventDO.EndDate.ToOffset(createdDate.Offset);
            if (existingAttendees == null)
                throw new ArgumentNullException("existingAttendees");
            var invitations = new List<InvitationDO>();
            if (eventDO.EventStatus == EventState.Booking)
            {
                invitations.AddRange(existingAttendees
                    .Union(newAttendees ?? Enumerable.Empty<AttendeeDO>())
                    .Select(newAttendee => _invitation.Generate(uow, InvitationType.InitialInvite, newAttendee, eventDO))
                    .Where(i => i != null));
            }
            else
            {
                if (newAttendees != null)
                {
                    invitations.AddRange(newAttendees.Select(newAttendee => _invitation.Generate(uow, InvitationType.InitialInvite, newAttendee, eventDO)).Where(i => i != null));
                }
                invitations.AddRange(existingAttendees.Select(existingAttendee => _invitation.Generate(uow, InvitationType.ChangeNotification, existingAttendee, eventDO)).Where(i => i != null));
            }

            return invitations;
        }

        private EventDO Update(IUnitOfWork uow, EventDO eventDO, EventDO updatedEventInfo, List<AttendeeDO> updatedAttendees, out List<AttendeeDO> newAttendees, out List<AttendeeDO> existingAttendees)
        {
            newAttendees = UpdateAttendees(uow, eventDO, updatedAttendees);
            if (newAttendees != null)
            {
                existingAttendees = updatedAttendees.Except(newAttendees).ToList();
            }
            else
            {
                existingAttendees = updatedAttendees.ToList();
            }
            eventDO = _mappingEngine.Map(updatedEventInfo, eventDO);
            if (newAttendees != null || uow.IsEntityModified(eventDO))
            {
                return eventDO;
            }
            else
            {
                return null;
            }
        }

        public List<AttendeeDO> UpdateAttendees(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> updatedAttendeeList)
        {
            List<AttendeeDO> newAttendees = new List<AttendeeDO>();

            var attendeesToDelete = eventDO.Attendees.Where(attendee => !updatedAttendeeList.Select(a => a.EmailAddress.Address).Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            foreach (var attendee in updatedAttendeeList.Where(att => !eventDO.Attendees.Select(a => a.EmailAddress.Address).Contains(att.EmailAddress.Address)))
            {
                newAttendees.Add(attendee);
            }
            return newAttendees.Any() ? newAttendees : null;
        }

        public EventDO AddAttendee(UserDO curUserDO, EventDO curEvent)
        {
            var curAttendee = new Attendee();
            var curAttendeeDO = curAttendee.Create(curUserDO);
            curEvent.Attendees.Add(curAttendeeDO);
            return curEvent;
        }

        public static iCalendar GenerateICSCalendarStructure(EventDO eventDO)
        {
            if (eventDO == null)
                throw new ArgumentNullException("eventDO");
            string fromEmail = ObjectFactory.GetInstance<IConfigRepository>().Get("fromEmail");
            string fromName = ObjectFactory.GetInstance<IConfigRepository>().Get("fromName");

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();

            //configure start and end time
            if (eventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(DateTime.SpecifyKind(eventDO.StartDate.ToUniversalTime().DateTime, DateTimeKind.Utc));
                dDayEvent.DTEnd = new iCalDateTime(DateTime.SpecifyKind(eventDO.EndDate.ToUniversalTime().DateTime, DateTimeKind.Utc));
            }
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
            ddayCalendar.Events.Add(dDayEvent);
            ddayCalendar.Method = CalendarMethods.Request;

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
                DateCreated = icsEvent.Created != null ? icsEvent.Created.UTC : DateTimeOffset.UtcNow,
                Attendees = icsEvent.Attendees
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