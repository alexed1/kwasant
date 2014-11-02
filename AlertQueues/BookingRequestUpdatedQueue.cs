using System;
using Data.Infrastructure;

namespace KwasantWeb.AlertQueues
{
    public class BookingRequestUpdatedQueue : ExpiringAlertQueue<BookingRequestUpdatedData>
    {
        public BookingRequestUpdatedQueue() : 
            base(TimeSpan.FromMinutes(5))
        {
            AlertManager.AlertConversationMemberAdded += id =>
            {
                if (FilterObjectID == id)
                    AppendItem(new BookingRequestUpdatedData());
            };
        }

        public override void ExpireItem(BookingRequestUpdatedData item)
        {
            //Do we email? Or do nothing.
        }
    }

    public class BookingRequestUpdatedData
    {
    }
}