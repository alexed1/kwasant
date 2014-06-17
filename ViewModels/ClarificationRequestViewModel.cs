using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces;

namespace KwasantWeb.ViewModels
{
    public class ClarificationRequestViewModel
    {
        public int Id { get; set; }
        public int BookingRequestId { get; set; }
        public string Recipients { get; set; }
        public string Question { get; set; }
        public int QuestionsCount { get; set; }
    }
}