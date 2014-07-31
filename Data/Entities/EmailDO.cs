using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Entities.Constants;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailDO : IEmail
    {
        [Key]
        public int Id { get; set; }
        
        public String Subject { get; set; }
        public String HTMLText { get; set; }
        public String PlainText { get; set; }
        public DateTimeOffset DateReceived { get; set; }
        public DateTimeOffset DateCreated { get; set; }

        [ForeignKey("EmailStatus")]
        public int EmailStatusID { get; set; }
        public EmailStatusRow EmailStatus { get; set; }
        
        [InverseProperty("Email")]
        public virtual List<RecipientDO> Recipients { get; set; }

        [InverseProperty("Email")]
        public virtual List<AttachmentDO> Attachments { get; set; }

        [InverseProperty("Emails")]
        public virtual List<EventDO> Events { get; set; }

        [ForeignKey("From"), Required]
        public int FromID { get; set; }
        public virtual EmailAddressDO From { get; set; }

        public IEnumerable<EmailAddressDO> To
        {
            get
            {
                return Recipients.Where(eea => eea.EmailParticipantTypeID == EmailParticipantType.To).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public IEnumerable<EmailAddressDO> BCC
        {
            get
            {
                return Recipients.Where(eea => eea.EmailParticipantTypeID == EmailParticipantType.Bcc).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public IEnumerable<EmailAddressDO> CC
        {
            get
            {
                return Recipients.Where(eea => eea.EmailParticipantTypeID == EmailParticipantType.Cc).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public EmailDO()
        {
            Recipients = new List<RecipientDO>();
            Attachments = new List<AttachmentDO>();
            Events = new List<EventDO>();
            DateCreated = DateTime.UtcNow;
            DateReceived = DateTime.UtcNow;
        }

        public void AddEmailRecipient(int type, EmailAddressDO emailAddress)
        {
            var newLink = new RecipientDO
            {
                Email = this,
                EmailAddress = emailAddress,
                EmailAddressID = emailAddress.Id,
                EmailID = Id,
                EmailParticipantTypeID = type
            };

            Recipients.Add(newLink);
            emailAddress.Recipients.Add(newLink);
        }
    }
}
