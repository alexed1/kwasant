using System;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IAttendee
    {
        [Key]
        int Id { get; set; }

      
        EmailAddressDO EmailAddress { get; set; }
        EventDO Event { get; set; }
        //TO DO add status and type
    }
}