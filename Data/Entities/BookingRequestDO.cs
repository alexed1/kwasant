using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Interfaces;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO, IBookingRequest
    {
        [Required]
        public virtual UserDO User { get; set; }

        public List<InstructionDO> Instructions { get; set; }
    }
}
