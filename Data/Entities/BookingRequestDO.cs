using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO, IBookingRequest
    {
        [Required]
        public virtual UserDO User { get; set; }

        public List<InstructionDO> Instructions { get; set; }

/*
        public virtual ClarificationRequestDO ClarificationRequest { get; set; }
*/

        [ForeignKey("BRState")]
        [Required]
        public int? BRStateID { get; set; }
        public virtual BookingRequestStatusDO BRState { get; set; }
    }
}
