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
        }

        [Required]
        public virtual UserDO User { get; set; }

        public List<InstructionDO> Instructions { get; set; }

        [InverseProperty("BookingRequest")]
        public IList<NegotiationDO> Negotiations { get; set; }

        [InverseProperty("BookingRequests")]
        public virtual List<CalendarDO> Calendars { get; set; }

        [Required, ForeignKey("BookingRequestStateTemplate")]
        public int BookingRequestState { get; set; }
        public virtual _BookingRequestStateTemplate BookingRequestStateTemplate { get; set; }
    }
}
