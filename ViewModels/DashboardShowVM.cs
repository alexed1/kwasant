using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KwasantWeb.ViewModels
{
    public class DashboardShowVM
    {
        public BookingRequestAdminVM BookingRequestVM { get; set; }
        public CalendarShowVM CalendarVM { get; set; }
    }
}