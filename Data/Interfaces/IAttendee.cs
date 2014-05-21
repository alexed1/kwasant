using System;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IAttendee
    {
        [Key]
        int Id { get; set; }

        String Name { get; set; }
        String EmailAddress { get; set; }
        EventDO Event { get; set; }
    }
}