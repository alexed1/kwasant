using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailDO : IEmail
    {
        [Key]
        public int Id { get; set; }
        
        public String Subject { get; set; }
        public String Text { get; set; }

        public virtual EmailStatus Status { get; set; }

        [InverseProperty("Email")]
        public virtual List<EmailEmailAddressDO> EmailEmailAddresses { get; set; }

        [InverseProperty("Email")]
        public virtual List<AttachmentDO> Attachments { get; set; }

        [InverseProperty("Emails")]
        public virtual List<EventDO> Events { get; set; }

        public EmailAddressDO From
        {
            get
            {
                return EmailEmailAddresses.Where(eea => eea.Type == EmailParticipantType.FROM).Select(eea => eea.EmailAddress).FirstOrDefault();
            }
        }

        public IEnumerable<EmailAddressDO> To
        {
            get
            {
                return EmailEmailAddresses.Where(eea => eea.Type == EmailParticipantType.TO).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public IEnumerable<EmailAddressDO> BCC
        {
            get
            {
                return EmailEmailAddresses.Where(eea => eea.Type == EmailParticipantType.BCC).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public IEnumerable<EmailAddressDO> CC
        {
            get
            {
                return EmailEmailAddresses.Where(eea => eea.Type == EmailParticipantType.CC).Select(eea => eea.EmailAddress).ToList();
            }
        }

        public EmailDO()
        {
            EmailEmailAddresses = new List<EmailEmailAddressDO>();
            Attachments = new List<AttachmentDO>();
            Events = new List<EventDO>();
        }

        public void AddEmailParticipant(EmailParticipantType type, EmailAddressDO emailAddress)
        {
            var newLink = new EmailEmailAddressDO
            {
                Email = this,
                EmailAddress = emailAddress,
                EmailAddressID = emailAddress.Id,
                EmailID = Id,
                Type = type
            };

            EmailEmailAddresses.Add(newLink);
            emailAddress.EmailEmailAddresses.Add(newLink);
        }
    }
}
