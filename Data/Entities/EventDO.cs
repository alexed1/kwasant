using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Data.Entities.Enumerations;

namespace Data.Entities
{
    public class EventDO
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }

        [NotMapped]
        public string ActivityStatus
        {
            get { return String.Empty; }
            set { throw new Exception("This field is reserved. You probably want to use 'State' instead."); }
        }

        private string _state { get; set; }
        public string State
        {
            get { return _state; }
            set
            {
                if (!StringEnumerations.EventState.Contains(value))
                {
                    throw new ApplicationException(
                        "tried to set State to an invalid value. See StringEnumerations class for allowable values and get approval before altering that set");
                }
                else
                {
                    _state = value;
                }
            }
        }

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


        public EventDO()
        {
            Attendees = new List<AttendeeDO>();
            Emails = new List<EmailDO>();
        }

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
