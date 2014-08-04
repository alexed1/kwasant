using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;

namespace Data.Entities
{
    public class NegotiationDO
    {
        #region Implementation of Negotiation

        [Key]
        public int Id { get; set; }

        [ForeignKey("NegotiationState")]
        public int NegotiationStateID { get; set; }
        public NegotiationStateRow NegotiationState { get; set; }
       
        public string Name { get; set; }

        [ForeignKey("BookingRequest")]
        public int BookingRequestID { get; set; }
        public BookingRequestDO BookingRequest { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<CalendarDO> Calendars { get; set; } 

        [ForeignKey("Email"), Required]
        public int RequestId { get; set; }
        public virtual EmailDO Email { get; set; }

        [InverseProperty("Negotiation")]
        public virtual List<AttendeeDO> Attendees { get; set; }

        [ForeignKey("NegotiationId")]
        public virtual List<QuestionDO> Questions { get; set; }


        public NegotiationDO ()
        {
            Questions = new List<QuestionDO>();
        }


        #endregion
    }
}
