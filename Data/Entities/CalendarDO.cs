using System;
using System.Collections.Generic;
using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{    
    public class CalendarDO : ICalendar
    {
        public CalendarDO()
        {
            Events = new List<EventDO>();
        }

        [Key]
        public int Id { get; set; }
        
        public String Name { get; set; }

        [ForeignKey("Owner")]
        public string OwnerID { get; set; }
        public virtual UserDO Owner { get; set; }

        [ForeignKey("ClarificationRequest")]
        public int? ClarificationRequestID { get; set; }
        public virtual ClarificationRequestDO ClarificationRequest { get; set; }

        [InverseProperty("Calendars")]
        public virtual List<BookingRequestDO> BookingRequests { get; set; }

        [InverseProperty("Calendar")]
        public virtual List<EventDO> Events { get; set; }

        [ForeignKey("Question")]
        public int? QuestionId { get; set; }
        public virtual QuestionDO Question { get; set; }
    }
}
