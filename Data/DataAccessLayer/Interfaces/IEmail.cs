using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Models;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IEmail
    {
        [Key]
        int EmailID { get; set; }

        //EmailAddress From { get; set; }
        String Subject { get; set; }
        String Text { get; set; }
        //IEnumerable<EmailAddress> To { get; set; }
        //IEnumerable<EmailAddress> BCC { get; set; }
        //IEnumerable<EmailAddress> CC { get; set; }
        //IEnumerable<Attachment> Attachments { get; set; }
        Invitation Invitation { get; set; }
    }
}