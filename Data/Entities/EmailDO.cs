using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailDO : IEmail
    {
        [Key]
        public int EmailID { get; set; }
        
        public String Subject { get; set; }
        public String Text { get; set; }

        [ForeignKey("Status")]
        public int StatusID { get; set; }
        public virtual EmailStatusDO Status { get; set; }

        public virtual EmailAddressDO From { get; set; }

        public virtual List<EmailAddressDO> To { get; set; }
        public virtual List<EmailAddressDO> CC { get; set; }
        public virtual List<EmailAddressDO> BCC { get; set; }

        [InverseProperty("Email")]
        public virtual List<AttachmentDO> Attachments { get; set; }

        [InverseProperty("Emails")]
        public virtual List<EventDO> Events { get; set; }
    }
}
