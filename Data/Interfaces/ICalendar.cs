using System;
using Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface ICalendar
    {
        [Key]
        int CalendarId { get; set; }

        String Name { get; set; }

        int PersonId { get; set; }

        PersonDO Owner { get; set; }        
        
    }
}