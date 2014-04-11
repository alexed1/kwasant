using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Invitation : IInvitation
    {
        [Key]
        public int InvitationID { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string Where { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual User CreatedBy { get; set; }
        public bool IsAllDay { get; set; }

        [InverseProperty("Invitation")]
        public virtual List<Attendee> Attendees { get; set; }

        [InverseProperty("Invitation")]
        public virtual List<Email> Emails { get; set; }
    }
}
