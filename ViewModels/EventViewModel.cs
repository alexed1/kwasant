using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;

using StructureMap;

namespace KwasantWeb.ViewModels
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public String Location { get; set; }
        public String Status { get; set; }
        public String Class { get; set; }
        public String Description { get; set; }
        public int Priority { get; set; }
        public int Sequence { get; set; }
        public String Summary { get; set; }
        public String Category { get; set; }
        public int BookingRequestID { get; set; }
        public String Attendees { get; set; }

        public String CreatedByName { get; set; }
        public String CreatedByID { get; set; }

        public EventViewModel()
        {

        }
        public EventViewModel(EventDO eventDO)
        {
            Id = eventDO.Id;
            StartDate = eventDO.StartDate;
            EndDate = eventDO.EndDate;
            IsAllDay = eventDO.IsAllDay;
            Location = eventDO.Location;
            Status = eventDO.Status;
            Class = eventDO.Class;
            Description = eventDO.Description;
            Priority = eventDO.Priority;
            Sequence = eventDO.Sequence;
            Summary = eventDO.Summary;
            Category = eventDO.Category;
            BookingRequestID = eventDO.BookingRequestID;
            Attendees = eventDO.Attendees == null ? String.Empty : string.Join(",", eventDO.Attendees.Select(e => e.EmailAddress));
            CreatedByName = eventDO.CreatedBy.UserName;
        }

        public static EventViewModel NewEventOnBookingRequest(int bookingRequestID, string start, string end)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var startDate = DateTime.Parse(start);
                var endDate = DateTime.Parse(end);

                var isAllDay = startDate.Equals(startDate.Date) && startDate.AddDays(1).Equals(endDate);
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestID);

                return new EventViewModel
                {
                    Attendees = String.Join(",", bookingRequestDO.Recipients.Select(eea => eea.EmailAddress.Address).Distinct()),
                    IsAllDay = isAllDay,
                    StartDate = startDate,
                    EndDate = endDate,
                    BookingRequestID = bookingRequestDO.Id
                };

            }
        }

        public void FillEventDO(IUnitOfWork uow, EventDO eventDO)
        {
            eventDO.StartDate = StartDate;
            eventDO.EndDate = EndDate;
            eventDO.IsAllDay = IsAllDay;
            eventDO.Location = Location;
            eventDO.Status = Status;
            eventDO.Class = Class;
            eventDO.Description = Description;
            eventDO.Priority = Priority;
            eventDO.Sequence = Sequence;
            eventDO.Summary = Summary;
            eventDO.Category = Category;
            eventDO.BookingRequestID = BookingRequestID;
            
            var attendees = Attendees.Split(',').ToList();

            var eventAttendees = eventDO.Attendees ?? new List<AttendeeDO>();
            var attendeesToDelete = eventAttendees.Where(attendee => !attendees.Contains(attendee.EmailAddress)).ToList();
            foreach(var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            foreach (var attendee in attendees.Where(att => !eventAttendees.Select(a => a.EmailAddress).Contains(att)))
            {
                var newAttendee = new AttendeeDO
                {
                    EmailAddress = attendee,
                    Event = eventDO,
                    EventID = eventDO.Id,
                    Name = attendee
                };
                uow.AttendeeRepository.Add(newAttendee);
            }
        }
    }
}
