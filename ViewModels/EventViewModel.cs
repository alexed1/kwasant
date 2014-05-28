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

        public UserDO CreatedBy { get; set; }
        public String CreatedByID { get; set; }

        public bool Confirmed { get; set; }

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
            Attendees = " disabled kw-163";
            //FIX THIS KW-163: Attendees = eventDO.Attendees == null ? String.Empty : string.Join(",", eventDO.Attendees.Select(e => e.EmailAddress.Address));
            CreatedBy = eventDO.CreatedBy;
            Confirmed = false;
        }

       

       
    }
}
