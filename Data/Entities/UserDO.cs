using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;
using StructureMap;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUser, ISaveHook, ICreateHook
    {
        public UserDO()
        {
            BookingRequests = new List<BookingRequestDO>();
            Calendars = new List<CalendarDO>();
            RemoteCalendarAuthData = new List<RemoteCalendarAuthDataDO>();
        }

        [InverseProperty("User")]
        public virtual IList<BookingRequestDO> BookingRequests { get; set; }

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
        public virtual IList<ProfileDO> Profiles { get; set; }

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

        void ISaveHook.BeforeSave()
        {
            
        }

        void ICreateHook.AfterCreate()
        {
            //we only want to treat explicit customers, who have sent us a BR, a welcome message
            //if there exists a booking request with this user as its created by...
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (uow.BookingRequestRepository.FindOne(br => br.User.Id == Id) != null)
                    AlertManager.ExplicitCustomerCreated(Id);
            }

            AlertManager.CustomerCreated(this);
        }
    }
}

