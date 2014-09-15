using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KwasantWeb.ViewModels
{
    public class CalendarShowVM
    {
        public List<int> LinkedCalendarIds { get; set; }
        public int ActiveCalendarId { get; set; }
        public string onDoneCallBack { get; set; }
        public string onCancelCallBack { get; set; }
    }
}