using System;
using Data.Infrastructure;

namespace KwasantWeb.AlertQueues
{
    public class NewBookingRequestForUserQueue : ExpiringAlertQueue<NewBookingRequestForUserQueueData>
    {
        public NewBookingRequestForUserQueue() 
            : base(TimeSpan.FromMinutes(5))
        {
            AlertManager.AlertNewBookingRequestForPreferredBooker +=
                (bookerID, bookingRequestID) =>
                    AppendItem(new NewBookingRequestForUserQueueData
                    {
                        UserID = bookerID,
                        BookingRequestID = bookingRequestID
                    });
        }

        public override void ExpireItem(NewBookingRequestForUserQueueData item)
        {
            //Now we email the person...
        }
    }

    public class NewBookingRequestForUserQueueData : IUserUpdateData
    {
        public string UserID { get; set; }
        public int BookingRequestID { get; set; }
    }
}