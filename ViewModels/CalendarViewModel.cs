using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViewModel.Models
{
    public class CalendarViewModel
    {
        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public String IsAllDay { get; set; }

        public String Location { get; set; }

        public String Status { get; set; }

        public String TransparencyType { get; set; }

        public String Class { get; set; }

        public String Description { get; set; }

        public int Priority { get; set; }

        public int Sequence { get; set; }

        public String Summary { get; set; }

        public String Category { get; set; }

        public int EventID { get; set; }

        public String Attendees { get; set; }
       
    }
}
