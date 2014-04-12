using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class EmailDO : IEmail
    {
        [Key]
        public int EmailID { get; set; }
        
        public String Subject { get; set; }
        public String Text { get; set; }

        public int StatusID { get; set; }
        public virtual EmailStatusDO StatusDO { get; set; }

        public virtual EmailAddressDO From { get; set; }

        [InverseProperty("ToEmail")]
        public virtual List<EmailAddressDO> To { get; set; }
        [InverseProperty("CCEmail")]
        public virtual List<EmailAddressDO> CC { get; set; }
        [InverseProperty("BCCEmail")]
        public virtual List<EmailAddressDO> BCC { get; set; }

        [InverseProperty("Email")]
        public virtual List<AttachmentDO> Attachments { get; set; }

        public virtual EventDO EventDo { get; set; }
    }
}
