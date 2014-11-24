using System;
using Data.Entities;
using Data.States;
using BookingRequestState = Data.States.BookingRequestState;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public BookingRequestDO TestBookingRequest1()
        {
            var curBookingRequestDO = new BookingRequestDO
                                          {
                                              Id = 1,
                                              From = TestEmailAddress1(),
                                              Subject = "Booking request subject",
                                              HTMLText = "Booking request text",
                                              EmailStatus = EmailState.Unprocessed,
                                              DateReceived = DateTimeOffset.UtcNow,
                                              State = BookingRequestState.AwaitingClient,
                                              Customer = TestUser1()
                                          };
            return curBookingRequestDO;
        }
    }
}
