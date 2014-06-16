using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUser
    {
        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }

        public String FirstName { get; set; }
        public String LastName { get; set; }

        [ForeignKey("EmailAddress")]
        public int EmailAddressID { get; set; }

        [Required]
        public virtual EmailAddressDO EmailAddress { get; set; }
    }
}

