using System;
using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{    
    public class CalendarDO : ICalendar
    {
        [Key]
        public int Id { get; set; }
        
        public String Name { get; set; }

        public int PersonId { get; set; }

        [ForeignKey("PersonId")]
        public virtual PersonDO Owner { get; set; }
        
    }
}
