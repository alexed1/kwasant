using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface ICustomer
    {
        [Key]
        int CustomerID { get; set; }

        String EmailAddress { get; set; }
        String FirstName { get; set; }
        String LastName { get; set; }
        UserDO User { get; set; }
        IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}