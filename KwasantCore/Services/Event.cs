using System;
using System.Linq;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Managers.CommunicationManager;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using Utilities;
using IEvent = Data.Interfaces.IEvent;

namespace KwasantCore.Services
{
    public class Event : IEvent
    {

        //this is called when a booker clicks on the calendar to create a new event. The form has not yet been filled out, so only 
        //some info about the event is known.
        public EventDO Create(EventDO curEventDO, IUnitOfWork uow)
        {
            curEventDO.IsAllDay = curEventDO.StartDate.Equals(curEventDO.StartDate.Date) &&
                                  curEventDO.StartDate.AddDays(1).Equals(curEventDO.EndDate);

            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(curEventDO.BookingRequestID);
            curEventDO.CreatedBy = bookingRequestDO.User;
            curEventDO.DateCreated = DateTimeOffset.UtcNow.ToOffset(bookingRequestDO.DateCreated.Offset);
            var curCalendar = bookingRequestDO.User.Calendars.FirstOrDefault();
            if (curCalendar == null)
                throw new EntityNotFoundException<CalendarDO>("No calendars found for this user.");
            curEventDO.Calendar = curCalendar;
            curEventDO.CalendarID = curCalendar.Id;
            curEventDO = AddAttendee(bookingRequestDO.User, curEventDO);
            curEventDO.StateID = EventState.Booking;

            return curEventDO;
        }

        public EventDO Create(IUnitOfWork uow, int bookingRequestID, string startDate, string endDate)
        {
            var curEventDO = new EventDO();
            curEventDO.StartDate = DateTime.Parse(startDate);
            curEventDO.EndDate = DateTime.Parse(endDate);
            curEventDO.BookingRequestID = bookingRequestID;
            return Create(curEventDO, uow);
        }


        //takes submitted form data and updates as necessary
        //in general, the new event data will simply overwrite the old data. 
        //in some cases, additional work is necessary to handle the changes
        public void Process(IUnitOfWork uow, EventDO eventDO)
        {            
            new CommunicationManager().DispatchInvitations(uow, eventDO);
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
            var remoteEvent = iCalendar.Events[0];
            return new EventDO()
            {
                Category = remoteEvent.Categories != null ? remoteEvent.Categories.FirstOrDefault() : null,
                Class = remoteEvent.Class,
                Description = remoteEvent.Description,
                IsAllDay = remoteEvent.IsAllDay,
                StartDate = remoteEvent.Start.UTC,
                EndDate = remoteEvent.End.UTC,
                Location = remoteEvent.Location,
                Sequence = remoteEvent.Sequence,
                Summary = remoteEvent.Summary,
                Transparency = remoteEvent.Transparency.ToString(),
                DateCreated = remoteEvent.Created.UTC,
                Attendees = remoteEvent.Attendees
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