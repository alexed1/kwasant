using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Enumerations;
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

/*
        public virtual ClarificationRequestDO ClarificationRequest { get; set; }
*/

        [InverseProperty("BookingRequests")]
        public virtual List<CalendarDO> Calendars { get; set; }

        [ForeignKey("BookingRequestState")]
        [Required]
        public int? BRState { get; set; }
        public virtual BookingRequestState BookingRequestState { get; set; }
    }
}
