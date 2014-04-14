using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class AttendeeDO : IAttendee
    {
        [Key]
        public int AttendeeID { get; set; }
        public String Name { get; set; }
        public String EmailAddress { get; set; }
        public virtual EventDO Event { get; set; }
    }
}
