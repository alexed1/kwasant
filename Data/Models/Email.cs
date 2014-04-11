using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Email : IEmail
    {
        [Key]
        public int EmailID { get; set; }
        
        public String Subject { get; set; }
        public String Text { get; set; }

        public int StatusID { get; set; }
        public virtual EmailStatus Status { get; set; }

        public virtual EmailAddress From { get; set; }

        [InverseProperty("ToEmail")]
        public virtual List<EmailAddress> To { get; set; }
        [InverseProperty("CCEmail")]
        public virtual List<EmailAddress> CC { get; set; }
        [InverseProperty("BCCEmail")]
        public virtual List<EmailAddress> BCC { get; set; }

        [InverseProperty("Email")]
        public virtual List<Attachment> Attachments { get; set; }

        public virtual Invitation Invitation { get; set; }
    }
}
