using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KwasantWeb.ViewModels
{
    public class ClarificationResponseViewModel
    {
        public int Id { get; set; }
        public int BookingRequestId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Question { get; set; }
        public string Response { get; set; }
    }
}