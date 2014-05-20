using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IUser
    {
        IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}
