using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO, IBookingRequest
    {
        public BookingRequestDO()
        {
            Calendars = new List<CalendarDO>();
            Negotiations = new List<NegotiationDO>();
        }

        [Required]
        public virtual UserDO User { get; set; }

        public virtual List<InstructionDO> Instructions { get; set; }

        [InverseProperty("BookingRequest")]
        public virtual IList<NegotiationDO> Negotiations { get; set; }

        [InverseProperty("BookingRequests")]
        public virtual List<CalendarDO> Calendars { get; set; }

        [Required, ForeignKey("BookingRequestStateTemplate")]
        public int State { get; set; }
        public virtual _BookingRequestStateTemplate BookingRequestStateTemplate { get; set; }

        [ForeignKey("Bookers")]
        public string BookerID { get; set; }
        public virtual UserDO Bookers { get; set; }
    }
}
