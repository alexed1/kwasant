using System.ComponentModel.DataAnnotations;
using Data.Models;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IBookingRequest : IEmail
    {
        [Required]
        Customer Customer { get; set; }
    }
}