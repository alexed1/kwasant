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
using IEvent = Data.Interfaces.IEvent;
using AutoMapper;

namespace KwasantCore.Services
{
    public class Event : IEvent
    {
        private readonly IMappingEngine _mappingEngine;

        public Event() : this(Mapper.Engine)
        {
            
        }

        public Event(IMappingEngine mappingEngine)
        {
            if (mappingEngine == null)
                throw new ArgumentNullException("mappingEngine");
            _mappingEngine = mappingEngine;
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

        public void Process(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> newAttendees, List<AttendeeDO> existingAttendees)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (eventDO == null)
                throw new ArgumentNullException("eventDO");
            if (newAttendees == null)
                throw new ArgumentNullException("newAttendees");
            if (existingAttendees == null)
                throw new ArgumentNullException("existingAttendees");

            var invitations = GenerateInvitations(uow, eventDO, newAttendees, existingAttendees);
            var invitation = new Invitation();
            foreach (var invitationDO in invitations)
            {
                invitation.Dispatch(uow, invitationDO);
            }
            eventDO.EventStatus = EventState.DispatchCompleted;
        }

        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes
        public void Process(IUnitOfWork uow, EventDO eventDO, EventDO updatedEventInfo, List<AttendeeDO> updatedAttendees)
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
                Process(uow, eventDO, newAttendees, existingAttendees);
            }
        }
     
        public List<InvitationDO> GenerateInvitations(IUnitOfWork uow, EventDO eventDO, List<AttendeeDO> newAttendees, List<AttendeeDO> existingAttendees)
        {
            //This line is so that the Server object is compiled. Without this, Razor fails; since it's executed at runtime and the object has been optimized out when running tests.
            //var createdDate = eventDO.BookingRequest.DateCreated;
            //eventDO.StartDate = eventDO.StartDate.ToOffset(createdDate.Offset);
            //eventDO.EndDate = eventDO.EndDate.ToOffset(createdDate.Offset);
            var invitations = new List<InvitationDO>();
            Invitation invitation = new Invitation();
            invitations.AddRange(newAttendees.Select(newAttendee => invitation.Generate(uow, InvitationType.InitialInvite, newAttendee, eventDO)).Where(i => i != null));
            invitations.AddRange(existingAttendees.Select(existingAttendee => invitation.Generate(uow, InvitationType.ChangeNotification, existingAttendee, eventDO)).Where(i => i != null));

            return invitations;
/*
            var t = Utilities.Server.ServerUrl;
            switch (eventDO.EventStatus)
            {
                case EventState.Booking:
                    {
                        eventDO.EventStatus = EventState.DispatchCompleted;

                        var calendar = Event.GenerateICSCalendarStructure(eventDO);
                        foreach (var attendeeDO in eventDO.Attendees)
                        {
                            var emailDO = CreateInvitationEmail(uow, eventDO, attendeeDO, false);
                            var email = new Email(uow, emailDO);
                            AttachCalendarToEmail(calendar, emailDO);
                            email.Send();
                        }

                        break;
                    }
                case EventState.ReadyForDispatch:
                case EventState.DispatchCompleted:
                    //Dispatched means this event was previously created. This is a standard event change. We need to figure out what kind of update message to send
                    if (EventHasChanged(uow, eventDO))
                    {
                        eventDO.EventStatus = EventState.DispatchCompleted;
                        var calendar = Event.GenerateICSCalendarStructure(eventDO);

                        var newAttendees = eventDO.Attendees.Where(a => a.Id == 0).ToList();

                        foreach (var attendeeDO in eventDO.Attendees)
                        {
                            //Id > 0 means it's an existing attendee, so we need to send the 'update' email to them.
                            var emailDO = CreateInvitationEmail(uow, eventDO, attendeeDO, !newAttendees.Contains(attendeeDO));
                            var email = new Email(uow, emailDO);
                            AttachCalendarToEmail(calendar, emailDO);
                            email.Send();
                        }
                    }
                    else
                    {
                        //If the event hasn't changed - we don't need a new email..?
                    }
                    break;

                case EventState.ProposedTimeSlot:
                    //Do nothing
                    break;
                default:
                    throw new Exception("Invalid event status");
            }
*/
        }

        private EventDO Update(IUnitOfWork uow, EventDO eventDO, EventDO updatedEventInfo, List<AttendeeDO> updatedAttendees, out List<AttendeeDO> newAttendees, out List<AttendeeDO> existingAttendees)
        {
            newAttendees = UpdateAttendees(uow, eventDO, updatedAttendees);
            existingAttendees = updatedAttendees.Except(newAttendees).ToList();
            eventDO = _mappingEngine.Map(updatedEventInfo, eventDO);
            if (/*newAttendees != null || */uow.IsEntityModified(eventDO))
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
            string fromEmail = ConfigRepository.Get("fromEmail");
            string fromName = ConfigRepository.Get("fromName");

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
                    Value = new Uri("mailto:" + attendee.EmailAddress),
                });
                attendee.Event = eventDO;
            }

            //final assembly of event
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };
            ddayCalendar.Events.Add(dDayEvent);
            ddayCalendar.Method = CalendarMethods.Request;

            return ddayCalendar;
        }

        public static EventDO CreateEventFromICSCalendar(iCalendar iCalendar)
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
                        EmailAddress = new EmailAddressDO(a.Value.OriginalString.Remove(0, a.Value.Scheme.Length + 1)),
                        Name = a.CommonName
                    })
                    .ToList(),
            };
        }

    }
}