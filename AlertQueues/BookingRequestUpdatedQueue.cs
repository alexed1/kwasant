using Data.Infrastructure;

namespace KwasantWeb.AlertQueues
{
    public class BookingRequestUpdatedQueue : PersonalAlertQueue<BookingRequestUpdatedData>
    {
        public BookingRequestUpdatedQueue(int bookingRequestID) 
        {
            AlertManager.AlertConversationMemberAdded += id =>
            {
                if (bookingRequestID == id)
                    AppendUpdate(new BookingRequestUpdatedData());
            };
        }
    }

    public class BookingRequestUpdatedData
    {
    }
}