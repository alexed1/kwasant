using System;

namespace KwasantWeb.AlertQueues
{
    public static class StaticQueues
    {
        public const String StrNewBookingRequestForUserQueue = @"NewBookingRequestForUserQueue";

        public static NewBookingRequestForUserQueue NewBookingRequestForUserQueue = new NewBookingRequestForUserQueue();

        public static IAlertQueue<IUserUpdateData> GetQueueByName(String name)
        {
            switch (name)
            {
                case StrNewBookingRequestForUserQueue:
                    return NewBookingRequestForUserQueue;
            }
            return null;
        }
    }
}