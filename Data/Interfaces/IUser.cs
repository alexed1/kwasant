using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IUser
    {
        string Id { get; set; }
        IList<BookingRequestDO> BookingRequests { get; set; }
        IEmailAddressDO EmailAddress { get; }

        [InverseProperty("User")]
        IList<RemoteCalendarAuthDataDO> RemoteCalendarAuthData { get; set; }

        bool IsRemoteCalendarAccessGranted(string providerName);
    }
}
