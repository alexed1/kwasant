using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class NegotiationDO
    {
        #region Implementation of Negotiation

        [Key]
        public int Id { get; set; }

        [ForeignKey("NegotiationStateTemplate")]
        public int NegotiationState { get; set; }
        public _NegotiationStateTemplate NegotiationStateTemplate { get; set; }
       
        public string Name { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<CalendarDO> Calendars { get; set; }

        [ForeignKey("BookingRequest"), Required]
        public int BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }

        //[InverseProperty("Negotiation")]
        //public virtual List<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Negotiation")]
        public virtual List<QuestionDO> Questions { get; set; }


        public NegotiationDO ()
        {
            Questions = new List<QuestionDO>();
        }


        #endregion
    }
}
