using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class CalendarViewModel
    {
        public int BookingRequestID { get; set; }
        public List<int> LinkedCalendarIDs { get; set; }
        public int MainCalendarID { get; set; }
    }
}