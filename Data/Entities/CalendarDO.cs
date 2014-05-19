using System;
using Data.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentValidation;
using FluentValidation.Validators;

namespace Data.Entities
{    
    public class CalendarDO : ICalendar
    {
        [Key]
        public int CalendarId { get; set; }
        
        public String Name { get; set; }

        public int PersonId { get; set; }       

        [ForeignKey("PersonId")]
        public virtual PersonDO Owner { get; set; }
        
    }
}
