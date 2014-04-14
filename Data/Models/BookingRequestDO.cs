using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class BookingRequestDO : EmailDO, IBookingRequest
    {
        [Required]
        public virtual CustomerDO Customer { get; set; }

        public List<InstructionDO> Instructions { get; set; }
    }
}
