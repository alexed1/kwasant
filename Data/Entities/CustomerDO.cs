using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Interfaces;

namespace Data.Entities
{
    public class CustomerDO : ICustomer
    {
        [Key]
        public int Id { get; set; }

        public String EmailAddress { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public virtual UserDO User { get; set; }
        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}
