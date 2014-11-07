using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;
using StructureMap;
using System.ComponentModel.DataAnnotations;
using Data.States.Templates;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUser, ISaveHook, ICreateHook, IBaseDO
    {
        [NotMapped]
        IEmailAddressDO IUser.EmailAddress
        {
            get { return EmailAddress; }
        }

        public UserDO()
        {
            UserBookingRequests = new List<BookingRequestDO>();
            BookerBookingRequests = new List<BookingRequestDO>();
            Calendars = new List<CalendarDO>();
            RemoteCalendarAuthData = new List<RemoteCalendarAuthDataDO>();
            Profiles = new List<ProfileDO>();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Boolean TestAccount { get; set; }

        //Booker only. Needs to be nullable otherwise DefaultValue doesn't work
        [Required, DefaultValue(true)]
        public bool? Available { get; set; }

        [ForeignKey("EmailAddress")]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        [Required, ForeignKey("UserStateTemplate")]
        public int? State { get; set; }
        public virtual _UserStateTemplate UserStateTemplate { get; set; }
        
        [InverseProperty("User")]
        public virtual IList<BookingRequestDO> UserBookingRequests { get; set; }

        [InverseProperty("Booker")]
        public virtual IList<BookingRequestDO> BookerBookingRequests { get; set; }

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

        public DateTimeOffset LastUpdated { get; set; }
    }
}

