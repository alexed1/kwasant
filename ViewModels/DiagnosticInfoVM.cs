using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class DiagnosticInfoVM
    {
        public string ServiceName { get; set; }
        public int Attempts { get; set; }
        public int Success { get; set; }
        public int Percent { get; set; }
        public String LastUpdated { get; set; }
        public List<DiagnosticEventInfoVM> Events { get; set; }
    }

    public class DiagnosticEventInfoVM
    {
        public String Date { get; set; }
        public String EventName { get; set; }
    }
}