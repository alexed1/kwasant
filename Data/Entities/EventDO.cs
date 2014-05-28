using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Data.Interfaces;
using IEvent = Data.Interfaces.IEvent;

namespace Data.Entities
{
    public class EventDO : IEvent
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }

        public string Status { get; set; }
        public string Transparency { get; set; }
        public string Class { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public int Sequence { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }

        [ForeignKey("CreatedBy"), Required]
        public string CreatedByID { get; set; }
        public virtual UserDO CreatedBy { get; set; }

        public bool IsAllDay { get; set; }

        [InverseProperty("Event")]
        public virtual List<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Events")]
        public virtual List<EmailDO> Emails { get; set; }

        [ForeignKey("BookingRequest")]
        public int BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }

        public void CopyFrom(EventDO eventDO)
        {
            //We can't called GetType() because EF mocks our object
            PropertyInfo[] props = typeof(EventDO).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(eventDO));
            }
        }

    }
}
