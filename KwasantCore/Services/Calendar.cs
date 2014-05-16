﻿using System;
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
        private BookingRequestDO _bookingRequestDO;
        private IUnitOfWork _uow;
        private readonly EventRepository _eventRepo;
        private EventValidator _curValidator;

        public Calendar(IUnitOfWork uow)
        {
            _uow = uow;
            
            _eventRepo = new EventRepository(_uow);
           
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

        public void LoadBookingRequest(BookingRequestDO curBR)
        {
            _bookingRequestDO = curBR;
            LoadData();
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
