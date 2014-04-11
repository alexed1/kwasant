using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Customer : ICustomer
    {
        [Key]
        public int CustomerID { get; set; }
        public virtual EmailAddress Email { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public virtual User User { get; set; }
        public virtual IEnumerable<BookingRequest> BookingRequests { get; set; }
    }
}
