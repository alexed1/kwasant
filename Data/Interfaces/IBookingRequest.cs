using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IBookingRequest : IEmail
    {
        [Required]
        CustomerDO Customer { get; set; }
    }
}