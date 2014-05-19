using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IBookingRequest : IEmail
    {
        [Required]
        UserDO User { get; set; }
    }
}