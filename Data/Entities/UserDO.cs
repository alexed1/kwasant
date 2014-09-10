using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Infrastructure;
using Data.Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUser, ISaveHook, ICreateHook
    {
        public UserDO()
        {
            BookingRequests = new List<BookingRequestDO>();
            Calendars = new List<CalendarDO>();
            RemoteCalendarAuthData = new List<RemoteCalendarAuthDataDO>();
            AddDefaultCalendar();
        }

        public virtual IEnumerable<BookingRequestDO> BookingRequests { get; set; }

        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Boolean TestAccount { get; set; }

        [ForeignKey("EmailAddress")]
        public int EmailAddressID { get; set; }

        public virtual EmailAddressDO EmailAddress { get; set; }

        [NotMapped]
        IEmailAddressDO IUser.EmailAddress
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
                     r.HasAccessToken());
        }

        public void AddDefaultCalendar()
        {
            if (this == null)
                throw new ArgumentNullException("curUser");

            if (!Calendars.Any())
            {
                var curCalendar = new CalendarDO
                {
                    Name = "Default Calendar",
                    Owner = this,
                    OwnerID = Id
                };
                Calendars.Add(curCalendar);
               
            }
        }

        void ISaveHook.BeforeSave()
        {
            
        }

        void ICreateHook.AfterCreate()
        {
            AlertManager.CustomerCreated(this.Id);
        }
    }
}

