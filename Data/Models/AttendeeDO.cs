using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class AttendeeDO : IAttendee
    {
        [Key]
        public int AttendeeID { get; set; }
        public String Name { get; set; }
        public String EmailAddress { get; set; }

        [ForeignKey("Invitation")]
        public int InvitationID { get; set; }
        public virtual InvitationDO Invitation { get; set; }
    }
}
