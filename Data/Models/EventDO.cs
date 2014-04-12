using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class EventDO : IInvitation
    {
        [Key]
        public int InvitationID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string Transparency { get; set; }
        public string Class { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public int Sequence { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        
        public virtual UserDO CreatedBy { get; set; }
        public bool IsAllDay { get; set; }

        [InverseProperty("Invitation")]
        public virtual List<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Invitation")]
        public virtual List<EmailDO> Emails { get; set; }
    }
}
