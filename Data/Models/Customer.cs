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
        public String Email { get; set; }
        public String Name { get; set; }
        public virtual User User { get; set; }
        public virtual IEnumerable<BookingRequest> BookingRequests { get; set; }
    }
}
