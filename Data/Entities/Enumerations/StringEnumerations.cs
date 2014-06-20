using System.Collections.Generic;

namespace Data.Entities.Enumerations
{
    public static class StringEnumerations
    {

        public static List<string> BookingStatus = new List<string>() {"Unprocessed", "Processed", "CheckedOut", "Invalid"};
        public static List<string> EventState = new List<string>() {"Booking", "ReadyForDispatch", "DispatchCompleted"};
    }
}
