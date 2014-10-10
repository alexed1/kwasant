using System.Collections.Generic;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class BookingRequestAdminVM
    {
        public List<int> ConversationMembers { get; set; }
        public int BookingRequestId { get; set; }
        public EmailDO CurEmailData { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }
        public string EmailAttachments { get; set; }
        public string Booker { get; set; } 
    }
}