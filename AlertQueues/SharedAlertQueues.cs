using System;
using System.Threading;

namespace KwasantWeb.AlertQueues
{
    public static class PersonalAlertQueues
    {
        //Define new queues with a string constant
        //Remember to update GetQueueByName
        public const String StrBookingRequestUpdatedQueue = @"BookingRequestUpdatedQueue";


        public static IPersonalAlertQueue GetQueueByName(String name)
        {
            switch (name)
            {
                case StrBookingRequestUpdatedQueue:
                    return new BookingRequestUpdatedQueue();
            }
            return null;
        }
    }

    public static class SharedAlertQueues
    {
        //Define new queues with a string constant, and a static instance of your queue.
        //Remember to update GetQueueByName, 
        //As well as the Begin() method (add your queue to 'staticQueues' array) - failing to update Begin will cause memory leaks!

        public const String StrBookingRequestReservedForUserQueue = @"BookingRequestReservedForUserQueue";
        public static BookingRequestReservedForUserQueue BookingRequestReservedForUserQueue = new BookingRequestReservedForUserQueue();

        public const String StrPollBookingRequestResponseQueue = @"PollBookingRequestResponseQueue";
        public static NewBookingRequestResponseQueue BookingRequestUpdatedQueue = new NewBookingRequestResponseQueue();

        public static ISharedAlertQueue<IUserUpdateData> GetQueueByName(String name)
        {
            switch (name)
            {
                case StrBookingRequestReservedForUserQueue:
                    return BookingRequestReservedForUserQueue;
                case StrPollBookingRequestResponseQueue:
                    return BookingRequestUpdatedQueue;
            }
            return null;
        }

        public static void Begin()
        {
            new Thread(() =>
            {
                var staticQueues = new IStaticQueue[] {BookingRequestReservedForUserQueue, BookingRequestUpdatedQueue};

                while (true)
                {
                    Thread.Sleep(60 * 1000);
                    foreach (var staticQueue in staticQueues)
                    {
                        staticQueue.PruneOldEntries();
                    }
                }
            }).Start();
        } 
    }
}