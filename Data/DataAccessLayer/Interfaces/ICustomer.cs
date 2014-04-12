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

        EmailAddressDO Email { get; set; }
        String FirstName { get; set; }
        String LastName { get; set; }
        UserDO UserDO { get; set; }
        IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}