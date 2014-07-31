using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Entities;
using Data.Entities.Constants;
using BookingRequestState = Data.Constants.BookingRequestState;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public BookingRequestDO TestBookingRequest1()
        {
            var curBookingRequestDO = new BookingRequestDO
                                          {
                                              Id = 1,
                                              DateCreated = DateTimeOffset.UtcNow,
                                              From = TestEmailAddress1(),
                                              Subject = "Booking request subject",
                                              HTMLText = "Booking request text",
                                              EmailStatusID = EmailStatus.Unprocessed,
                                              DateReceived = DateTimeOffset.UtcNow,
                                              BRState = BookingRequestState.Pending,
                                              User = TestUser()
                                          };
            return curBookingRequestDO;
        }
    }
}
