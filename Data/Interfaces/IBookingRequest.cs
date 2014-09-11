using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IBookingRequestDO : IEmail
    {
        [Required]
        UserDO User { get; set; }

      
    }

    public interface IBookingRequest 
    {
        void ExtractEmailAddresses(IUnitOfWork uow, EventDO eventDO);
    }

}