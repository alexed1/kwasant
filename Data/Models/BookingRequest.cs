using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class BookingRequest : Email, IBookingRequest
    {
        [Required]
        public virtual Customer Customer { get; set; }
    }
}
