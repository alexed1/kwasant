using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Invitation : IInvitation
    {
        [Key]
        public int InvitationID { get; set; }
        public string Description { get; set; }
        public string Where { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}
