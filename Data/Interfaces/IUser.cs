using System;
using Data.Entities;
using System.ComponentModel;
using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface IUser
    {
        string Password { get; set;}
        IEnumerable<BookingRequestDO> BookingRequests { get; set; }
    }
}

