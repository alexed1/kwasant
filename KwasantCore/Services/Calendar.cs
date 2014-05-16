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
using Data.Validators;
using KwasantCore.Managers.CommunicationManager;
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
        private IUnitOfWork _uow;
        private readonly EventRepository _eventRepo;
        private EventValidator _curValidator;

        public Calendar(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            _uow = uow;
            _bookingRequestDO = bookingRequest;
            _eventRepo = new EventRepository(_uow);
            LoadData();
           _curValidator = new EventValidator();
            
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
                .Where(curEventDO => curEventDO.BookingRequest.Customer.CustomerID == _bookingRequestDO.Customer.CustomerID)
                .ToDictionary(
                    curEventDO => curEventDO.EventID,
                    curEventDO => curEventDO);

        }

        public void Reload()
        {
            LoadData();
        }

        //public void DispatchEvent(EventDO curEventDO)
        //{
        //    DispatchEvent(curEventDO);
            
        //}

        public void DispatchEvent(EventDO curEventDO)
        {
            if(curEventDO.Attendees == null)
                curEventDO.Attendees = new List<AttendeeDO>();

            Email email = new Email(_uow, curEventDO);
            EmailDO outboundEmail = email.EmailDO;
            string fromEmail = CommunicationManager.GetFromEmail();
            string fromName = CommunicationManager.GetFromName(); 

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
            if (curEventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(curEventDO.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(curEventDO.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            dDayEvent.Location = curEventDO.Location;
            dDayEvent.Description = curEventDO.Description;
            dDayEvent.Summary = curEventDO.Summary;
            foreach (AttendeeDO attendee in curEventDO.Attendees)
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
                attendee.Event = curEventDO;
            }
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);

            AttachCalendarToEmail(ddayCalendar, outboundEmail);

            if (curEventDO.Emails == null)
                curEventDO.Emails = new List<EmailDO>();
            curEventDO.Emails.Add(outboundEmail);

            _uow.SaveChanges();
            Reload();
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

        public void AddEvent(EventDO curEventDO)
        {
            if (_bookingRequestDO.Events == null)
                _bookingRequestDO.Events = new List<EventDO>();
            _bookingRequestDO.Events.Add(curEventDO);

            curEventDO.BookingRequest = _bookingRequestDO;

            _eventRepo.Add(curEventDO);
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
