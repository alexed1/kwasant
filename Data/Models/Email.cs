using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mail;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using StructureMap;

namespace Data.Models
{
    public class Email : IEmail
    {
        

        [Key]
        public int EmailID { get; set; }
        
        public String Subject { get; set; }
        public String Text { get; set; }

        [ForeignKey("Status")]
        public int StatusID;
        public virtual EmailStatus Status { get; set; }

        public virtual EmailAddress From { get; set; }

        [InverseProperty("ToEmail")]
        public virtual List<EmailAddress> To { get; set; }
        [InverseProperty("CCEmail")]
        public virtual List<EmailAddress> CC { get; set; }
        [InverseProperty("BCCEmail")]
        public virtual List<EmailAddress> BCC { get; set; }

        public virtual List<Attachment> Attachments { get; set; }
        public virtual Invitation Invitation { get; set; }
    }
}
