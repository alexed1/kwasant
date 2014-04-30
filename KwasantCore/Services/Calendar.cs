using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using UtilitiesLib;

namespace KwasantCore.Services
{
    /// <summary>
    /// Summary description for EventManager
    /// </summary>
    public class Calendar
    {
        private readonly BookingRequestDO _bookingRequestDO;
        private readonly IUnitOfWork _uow;
        private readonly EventRepository _eventRepo;

        public Calendar(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            _uow = uow;
            _bookingRequestDO = bookingRequest;
            _eventRepo = new EventRepository(_uow);
            LoadData();
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _uow;
            }
        }

        private Dictionary<int, EventDO> _events;
        public List<EventDO> EventsList
        {
            get
            {
                return _events.Values.ToList();
            }
        }

        private void LoadData()
        {
            IEnumerable<EventDO> test = _eventRepo.GetAll();
            _events = _eventRepo.GetQuery()
                .Where(eventDO => eventDO.BookingRequest.Customer.CustomerID == _bookingRequestDO.Customer.CustomerID)
                .ToDictionary(
                    eventDO => eventDO.EventID,
                    eventDO => eventDO);

        }

        public void Reload()
        {
            LoadData();
        }

        public void DispatchEvent(EventDO eventDO)
        {
            DispatchEvent(_uow, eventDO);
            Reload();
        }

        public static void DispatchEvent(IUnitOfWork uow, EventDO eventDO)
        {
            if(eventDO.Attendees == null)
                eventDO.Attendees = new List<AttendeeDO>();

            string fromEmail = "lucreorganizer@gmail.com";
            string fromName = "Booqit Organizer";

            EmailDO outboundEmail = new EmailDO();
            outboundEmail.From = new EmailAddressDO {Address = fromEmail, Name = fromName};
            outboundEmail.To = eventDO.Attendees.Select(a => new EmailAddressDO { Address = a.EmailAddress, Name = a.Name}).ToList();
            outboundEmail.Subject = "Invitation via Booqit: " + eventDO.Summary + "@ " + eventDO.StartDate;
            outboundEmail.Text = "This is a Booqit Event Request. For more information, see https://foo.com";
            outboundEmail.Status = EmailStatus.QUEUED;

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
            if (eventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(eventDO.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(eventDO.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            dDayEvent.Location = eventDO.Location;
            dDayEvent.Description = eventDO.Description;
            dDayEvent.Summary = eventDO.Summary;
            foreach (AttendeeDO attendee in eventDO.Attendees)
            {
                dDayEvent.Attendees.Add(new Attendee
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
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);

            AttachCalendarToEmail(ddayCalendar, outboundEmail);

            if (eventDO.Emails == null)
                eventDO.Emails = new List<EmailDO>();
            eventDO.Emails.Add(outboundEmail);

            uow.SaveChanges();
        }

        private static void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = Email.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/calendar", Name = "invite.ics" }
                ));

            if(emailDO.Attachments == null)
                emailDO.Attachments = new List<AttachmentDO>();
            
            attachmentDO.Email = emailDO;
            emailDO.Attachments.Add(attachmentDO);
        }

        public EventDO GetEvent(int eventID)
        {
            return _events[eventID];
        }

        public void AddEvent(EventDO eventDO)
        {
            if (_bookingRequestDO.Events == null)
                _bookingRequestDO.Events = new List<EventDO>();
            _bookingRequestDO.Events.Add(eventDO);

            eventDO.BookingRequest = _bookingRequestDO;

            _eventRepo.Add(eventDO);
            _uow.SaveChanges();
            Reload();
        }

        public void DeleteEvent(int id)
        {
            EventDO eventToDelete = EventsList.FirstOrDefault(inv => inv.EventID == id);
            if (eventToDelete != null)
            {
                _eventRepo.Remove(eventToDelete);
                _uow.SaveChanges();
            }
            Reload();
        }

        public void DeleteEvent(String idStr)
        {
            int id = idStr.ToInt();
            DeleteEvent(id);
        }

        public void MoveEvent(int id, DateTime newStart, DateTime newEnd)
        {
            EventDO itemToMove = EventsList.FirstOrDefault(inv => inv.EventID == id);
            if (itemToMove != null)
            {
                itemToMove.StartDate = newStart;
                itemToMove.EndDate = newEnd;
                _uow.SaveChanges();
            }
            Reload();
        }

        public void MoveEvent(String idStr, DateTime newStart, DateTime newEnd)
        {
            int id = Int32.Parse(idStr);
            MoveEvent(id, newStart, newEnd);
        }
    }
}
