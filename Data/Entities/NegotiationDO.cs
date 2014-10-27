using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using Microsoft.AspNet.Identity;
using NegotiationState = Data.States.NegotiationState;

namespace Data.Entities
{
    public class NegotiationDO
    {
        public NegotiationDO()
        {
            Questions = new List<QuestionDO>();
            Attendees = new List<AttendeeDO>();

            NegotiationState = States.NegotiationState.InProcess;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }

        [ForeignKey("NegotiationStateTemplate")]
        public int? NegotiationState { get; set; }
        public _NegotiationStateTemplate NegotiationStateTemplate { get; set; }
       
        [ForeignKey("BookingRequest"), Required]
        public int? BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<CalendarDO> Calendars { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<QuestionDO> Questions { get; set; }
    }
}
