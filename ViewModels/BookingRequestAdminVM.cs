using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class BookingRequestAdminVM
    {
        public List<int> ConversationMembers { get; set; }
        public int BookingRequestId { get; set; }
        public EmailDO CurEmailData { get; set; }
    }
}