using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
            RemoteCalendarAuthData = new List<RemoteCalendarAuthDataDO>();
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

        [InverseProperty("Owner")]
        public virtual IList<CalendarDO> Calendars { get; set; }

        [InverseProperty("User")]
        public virtual IList<RemoteCalendarAuthDataDO> RemoteCalendarAuthData { get; set; }

        public bool IsRemoteCalendarAccessGranted(string providerName)
        {
            return RemoteCalendarAuthData
                .Any(r =>
                     r.Provider != null &&
                     r.Provider.Name == providerName &&
                     !string.IsNullOrEmpty(r.AuthData) &&
                     r.AuthData.Contains("access_token"));
        }
    }
}

