using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailDO : IEmail
    {
        [Key]
        public int EmailID { get; set; }
        
        public String Subject { get; set; }
        public String HTMLText { get; set; }
        public String PlainText { get; set; }

        public virtual EmailStatus Status { get; set; }

        public virtual EmailAddressDO From { get; set; }

        public virtual List<EmailAddressDO> To { get; set; }
        public virtual List<EmailAddressDO> CC { get; set; }
        public virtual List<EmailAddressDO> BCC { get; set; }

        [InverseProperty("Email")]
        public virtual List<AttachmentDO> Attachments { get; set; }

        [InverseProperty("Emails")]
        public virtual List<EventDO> Events { get; set; }


        public EmailDO()
        {
            To = new List<EmailAddressDO>();
            CC = new List<EmailAddressDO>();
            BCC = new List<EmailAddressDO>();
            Attachments = new List<AttachmentDO>();
            Events = new List<EventDO>();
        }
    }
}
