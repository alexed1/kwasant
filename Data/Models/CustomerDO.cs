using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class CustomerDO : ICustomer
    {
        [Key]
        public int CustomerID { get; set; }
        public virtual EmailAddressDO Email { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public virtual UserDO UserDO { get; set; }
        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}
