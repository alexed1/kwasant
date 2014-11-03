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

        public const String StrNewBookingRequestForUserQueue = @"NewBookingRequestForUserQueue";
        public static NewBookingRequestForUserQueue NewBookingRequestForUserQueue = new NewBookingRequestForUserQueue();


        public static ISharedAlertQueue<IUserUpdateData> GetQueueByName(String name)
        {
            switch (name)
            {
                case StrNewBookingRequestForUserQueue:
                    return NewBookingRequestForUserQueue;
            }
            return null;
        }

        public static void Begin()
        {
            new Thread(() =>
            {
                var staticQueues = new IStaticQueue[] {NewBookingRequestForUserQueue};

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