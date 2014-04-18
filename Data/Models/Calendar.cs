using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DDay.DDay.iCal;
using Data.DDay.DDay.iCal.DataTypes;
using Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers;
using Data.Tools;
using DDay.DDay.iCal.Components;
using UtilitiesLib;

namespace Data.Models
{
    /// <summary>
    /// Summary description for EventManager
    /// </summary>
    public class Calendar
    {
        private readonly BookingRequestDO _bookingRequestDO;
        private readonly IUnitOfWork _uow;
        private EventRepository _eventRepo;
        
        public Calendar(IUnitOfWork uow, BookingRequestDO bookingRequestDO)
        {
            _uow = uow;
            _bookingRequestDO = bookingRequestDO;
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
            _events = _eventRepo.GetQuery().Where(i => i.BookingRequest.Customer.CustomerID == _bookingRequestDO.Customer.CustomerID).ToDictionary(e => e.EventID, e => e);
        }

        private void SaveData()
        {
            _uow.Db.SaveChanges();
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

        public static void DispatchEvent(IUnitOfWork uow, EventDO eventDo)
        {
            if(eventDo.Attendees == null)
                eventDo.Attendees = new List<AttendeeDO>();

            string fromEmail = "lucreorganizer@gmail.com";
            string fromName = "Booqit Organizer";

            EmailDO outboundEmail = new EmailDO();
            outboundEmail.From = new EmailAddressDO {Address = fromEmail, Name = fromName};
            outboundEmail.To = eventDo.Attendees.Select(a => new EmailAddressDO { Address = a.EmailAddress, Name = a.Name}).ToList();
            outboundEmail.Subject = "Invitation via Booqit: " + eventDo.Summary + "@ " + eventDo.StartDate;
            outboundEmail.Text = "This is a Booqit Event Request. For more information, see https://foo.com";
            outboundEmail.StatusID = EmailStatusConstants.QUEUED;

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
            if (eventDo.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(eventDo.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(eventDo.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            dDayEvent.Location = eventDo.Location;
            dDayEvent.Description = eventDo.Description;
            dDayEvent.Summary = eventDo.Summary;
            foreach (AttendeeDO attendee in eventDo.Attendees)
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
                attendee.Event = eventDo;
            }
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);

            AttachCalendarToEmail(ddayCalendar, outboundEmail);

            if (eventDo.Emails == null)
                eventDo.Emails = new List<EmailDO>();
            eventDo.Emails.Add(outboundEmail);

            uow.SaveChanges();
        }

        private static void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = EmailHelper.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/calendar", Name = "invite.ics" }
                ));

            if(emailDO.Attachments == null)
                emailDO.Attachments = new List<AttachmentDO>();

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
            if (eventToDelete != null && !_eventRepo.IsDetached(eventToDelete))
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
