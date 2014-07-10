using System.Collections.Generic;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IUser
    {
        string Id { get; set; }
        IEnumerable<BookingRequestDO> BookingRequests { get; set; }
        string GoogleAuthData { get; set; }
        bool GrantedAccessToGoogleCalendar { get; }
        IEmailAddress EmailAddress { get; }
    }
}
