using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUser
    {
        public UserDO()
        {
            BookingRequests = new List<BookingRequestDO>();
            Calendars = new List<CalendarDO>();
        }

        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }

        public String FirstName { get; set; }
        public String LastName { get; set; }

        [ForeignKey("EmailAddress")]
        public int EmailAddressID { get; set; }

        public virtual EmailAddressDO EmailAddress { get; set; }

        [NotMapped]
        IEmailAddress IUser.EmailAddress
        {
            get { return EmailAddress; }
        }

        public string GoogleAuthData { get; set; }

        [NotMapped]
        public bool GrantedAccessToGoogleCalendar { get { return !string.IsNullOrEmpty(GoogleAuthData) && GoogleAuthData.Contains("access_token"); } }

        [InverseProperty("Owner")]
        public virtual IList<CalendarDO> Calendars { get; set; } 
    }
}

