using System;
using System.ComponentModel.DataAnnotations;
using Data.Models;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IAttendee
    {
        [Key]
        int AttendeeID { get; set; }

        String Name { get; set; }
        String EmailAddress { get; set; }
        Invitation Invitation { get; set; }
    }
}