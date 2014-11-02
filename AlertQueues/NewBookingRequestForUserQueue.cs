using Data.Infrastructure;

namespace KwasantWeb.AlertQueues
{
    public class NewBookingRequestForUserQueue : SharedSharedAlertQueue<NewBookingRequestForUserQueueData>
    {
        public NewBookingRequestForUserQueue() 
        {
            AlertManager.AlertNewBookingRequestForPreferredBooker +=
                (bookerID, bookingRequestID) =>
                    AppendUpdate(new NewBookingRequestForUserQueueData
                    {
                        UserID = bookerID,
                        BookingRequestID = bookingRequestID
                    });
        }

        protected override void ObjectExpired(NewBookingRequestForUserQueueData item)
        {
            var t = 0;
            //Now we email the person...
        }
    }

    public class NewBookingRequestForUserQueueData : IUserUpdateData
    {
        public string UserID { get; set; }
        public int BookingRequestID { get; set; }
    }
}