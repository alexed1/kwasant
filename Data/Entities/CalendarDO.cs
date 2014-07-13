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
        
        [InverseProperty("Calendar")]
        public virtual IList<EventDO> Events { get; set; }

        public DateTimeOffset LastSynchronized { get; set; }
    }
}
