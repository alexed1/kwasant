using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IEmail
    {
        [Key]
        int Id { get; set; }

        //EmailAddress From { get; set; }
        String Subject { get; set; }
        String HTMLText { get; set; }
        //IEnumerable<EmailAddress> To { get; set; }
        //IEnumerable<EmailAddress> BCC { get; set; }
        //IEnumerable<EmailAddress> CC { get; set; }
        //IEnumerable<Attachment> Attachments { get; set; }
        List<EventDO> Events { get; set; }
    }
}