using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;
using Data.Interfaces;

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

        [Required, ForeignKey("BookingRequestStateRow")]
        public int BookingRequestStateID { get; set; }
        public virtual BookingRequestStateRow BookingRequestStateRow { get; set; }
    }
}
