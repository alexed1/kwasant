namespace Data.States
{
    public class BookingRequestState
    {
        //public const int Unprocessed = 1;
        //public const int Processed = 2;
        //public const int CheckedOut = 3;
        //public const int Invalid = 4;
        //public const int Pending = 5;

        public const int Unstarted = 1;
        public const int Resolved = 2;
        public const int Booking = 3;
        public const int Invalid = 4;
        public const int AwaitingClient = 5;
        public const int NeedsBooking = 6;
    }
}
