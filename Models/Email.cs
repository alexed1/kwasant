using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Windows.Forms;
using Shnexy.DataAccessLayer.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Shnexy.Models
{
    
    public class Email : IEmail
    {
        [Key]
        public int Id { get; set; }

        public string Body { get; set; }

        public string Subject { get; set; }
        public EmailAddress Sender { get; set; }

        public ICollection<EmailAddress> To_Addresses { get; set; }

        public ICollection<EmailAddress> CC_Addresses { get; set; }

        public ICollection<EmailAddress> Bcc_Addresses { get; set; }

        public string Status { get; set; } //TODO replace with typesafe enum

        private EmailAddress curAddress ;
        private IEmailRepository _emailRepo;

        public Email()
        {
        }

        public Email(MailMessage curMessage, IEmailRepository emailRepo)
        {
            Body = curMessage.Body;
            Subject = curMessage.Subject;
            Sender = MapAddress(curMessage.From);
     
            foreach (var address in curMessage.To)
            {
                To_Addresses = new Collection<EmailAddress>();
                To_Addresses.Add(MapAddress(address)); //ugly This maps the MSDN address to our normalized EmailAddress
            }
            foreach (var address in curMessage.CC)
            {
                CC_Addresses = new Collection<EmailAddress>();
                CC_Addresses.Add(MapAddress(address)); //ugly This maps the MSDN address to our normalized EmailAddress
            }
            foreach (var address in curMessage.Bcc)
            {
                Bcc_Addresses = new Collection<EmailAddress>();
                Bcc_Addresses.Add(MapAddress(address)); //ugly This maps the MSDN address to our normalized EmailAddress
            }
            Status = "Unprocessed";
            _emailRepo = emailRepo;
        }

        public EmailAddress MapAddress(MailAddress importedAddress)
        {
            return new EmailAddress(importedAddress);
        }

        public void Save()
        {
            _emailRepo.Add(this);
            _emailRepo.UnitOfWork.SaveChanges(); 
        }

    }
}