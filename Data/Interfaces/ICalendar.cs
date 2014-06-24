using System;
using Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface ICalendar
    {
        [Key]
        int Id { get; set; }

        String Name { get; set; }

        int PersonId { get; set; }

        UserDO Owner { get; set; }        
        
    }
}