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
using Utilities;

namespace KwasantCore.Services
{
    /// <summary>
    /// Summary description for EventManager
    /// </summary>
    public class Calendar 
    {
        private BookingRequestDO _bookingRequestDO;
        private IUnitOfWork _uow;

        public Calendar(IUnitOfWork uow)
        {
            _uow = uow;
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

        public void LoadBookingRequest(BookingRequestDO curBR)
        {
            _bookingRequestDO = curBR;
            LoadData();
        }

        private void LoadData()
        {
            _events = _uow.EventRepository.GetQuery()
                .Where(curEventDO => curEventDO.BookingRequest.User.Id == _bookingRequestDO.User.Id)
                .ToDictionary(
                    curEventDO => curEventDO.Id,
                    curEventDO => curEventDO);

        }

        public void Reload()
        {
            LoadData();
        }

            

        public void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
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

            _uow.EventRepository.Add(curEventDO);
            _uow.SaveChanges();
            Reload();
        }

        public void DeleteEvent(int id)
        {
            EventDO eventToDelete = EventsList.FirstOrDefault(inv => inv.Id == id);
            if (eventToDelete != null)
            {
                _uow.EventRepository.Remove(eventToDelete);
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
            EventDO itemToMove = EventsList.FirstOrDefault(inv => inv.Id == id);
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
