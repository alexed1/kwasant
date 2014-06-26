using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.Enumerations;

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
                                              EmailStatus = EmailStatus.UNPROCESSED,
                                              DateReceived = DateTimeOffset.UtcNow,
                                              BookingStatus = "Unprocessed",
                                              User = TestUser()
                                          };
            return curBookingRequestDO;
        }
    }
}
