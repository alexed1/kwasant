using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class InvitationDO : IInvitation
    {
        [Key]
        public int InivitationID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }

        [ForeignKey("Status")]
        public int StatusID { get; set; }
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

        [InverseProperty("Invitations")]
        public virtual List<EmailDO> Emails { get; set; }

        public virtual BookingRequestDO BookingRequest { get; set; }

        public void CopyFrom(InvitationDO invitationDO)
        {
            //We can't called GetType() because EF mocks our object
            PropertyInfo[] props = typeof(InvitationDO).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(invitationDO));
            }
        }

    }
}
