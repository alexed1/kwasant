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

        String Email { get; set; }
        String Name { get; set; }
        User User { get; set; }
        IEnumerable<BookingRequest> BookingRequests { get; set; }
    }
}