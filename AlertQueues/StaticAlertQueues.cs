using System;
using System.Runtime.Remoting.Channels;
using System.Threading;

namespace KwasantWeb.AlertQueues
{
    public static class StaticAlertQueues
    {
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