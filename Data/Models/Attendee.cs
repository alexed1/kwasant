using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Attendee : IAttendee
    {
        [Key]
        public int AttendeeID { get; set; }
        public String Name { get; set; }
        public String EmailAddress { get; set; }
        public bool Organiser { get; set; }
        public virtual Invitation Invitation { get; set; }
    }
}
