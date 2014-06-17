using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO, IBookingRequest
    {
        [Required]
        public virtual UserDO User { get; set; }

        public List<InstructionDO> Instructions { get; set; }

        private string _bookingStatus;

        [Required]
        public string BookingStatus {
            get { return _bookingStatus; }
            set
            {
                if (!StringEnumerations.BookingStatus.Contains(value))
                {
                    throw new ApplicationException(
                        "tried to set BookingStatus to an invalid value. See StringEnumerations class for allowable values and get approval before altering that set");
                }
                else
                {
                    _bookingStatus = value;
                }
            } 
        }
    }
}
