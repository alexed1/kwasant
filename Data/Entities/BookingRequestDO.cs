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

        [ForeignKey("BookingRequestStatus")]
        public int BRState { get; set; }
      //  [Required]   VERIFY THAT IT IS OK TO COMMENT THIS OUT
        public virtual BookingRequestStatusDO BookingRequestStatus { get; set; }
    }
}
