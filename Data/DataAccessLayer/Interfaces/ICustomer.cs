using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Models;

namespace Data.DataAccessLayer.Interfaces
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